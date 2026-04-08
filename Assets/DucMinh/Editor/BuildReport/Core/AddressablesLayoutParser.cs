using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DucMinh.BuildReport
{
    /// <summary>
    /// Parses the Addressables build layout report file.
    /// Falls back gracefully if Addressables package is not present or layout file doesn't exist.
    /// </summary>
    public static class AddressablesLayoutParser
    {
        private const string LayoutFolder = "Library/com.unity.addressables";
        private const string LayoutFilePattern = "buildlayout*.txt";

        public static (List<AddressablesGroupData> groups, List<DuplicationData> duplications) Parse()
        {
            var groups       = new List<AddressablesGroupData>();
            var duplications = new List<DuplicationData>();

            // Graceful fallback: if no Addressables layout exists, return empty
            if (!Directory.Exists(LayoutFolder))
                return (groups, duplications);

            var files = Directory.GetFiles(LayoutFolder, LayoutFilePattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                // Try JSON variant
                files = Directory.GetFiles(LayoutFolder, "buildlayout*.json", SearchOption.TopDirectoryOnly);
            }

            if (files.Length == 0)
                return (groups, duplications);

            // Use the most recent layout file
            var layoutFile = files.OrderByDescending(File.GetLastWriteTimeUtc).First();

            try
            {
                TryParseViaReflection(layoutFile, groups, duplications);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildReport] Could not parse Addressables build layout: {ex.Message}");
            }

            return (groups, duplications);
        }

        /// <summary>
        /// Uses reflection to access the Addressables Build Layout API so this code compiles
        /// even when Addressables package is not installed.
        /// </summary>
        private static void TryParseViaReflection(string layoutFile,
            List<AddressablesGroupData> groups,
            List<DuplicationData> duplications)
        {
            // Find BuildLayout type via reflection
            var buildLayoutType = FindType("UnityEditor.AddressableAssets.Build.Layout.BuildLayout");
            if (buildLayoutType == null)
            {
                ParseTextLayout(layoutFile, groups);
                return;
            }

            // BuildLayout.Open(path) or similar factory
            var openMethod = buildLayoutType.GetMethod("Open",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (openMethod == null)
            {
                ParseTextLayout(layoutFile, groups);
                return;
            }

            object layout = openMethod.Invoke(null, new object[] { layoutFile });
            if (layout == null) return;

            // Read Groups
            var groupsProp = buildLayoutType.GetProperty("Groups");
            var duplicatedProp = buildLayoutType.GetProperty("DuplicatedAssets");

            if (groupsProp?.GetValue(layout) is System.Collections.IList rawGroups)
                ParseGroupsFromReflection(rawGroups, groups);

            if (duplicatedProp?.GetValue(layout) is System.Collections.IList rawDups)
                ParseDuplicationsFromReflection(rawDups, duplications);
        }

        private static void ParseGroupsFromReflection(System.Collections.IList rawGroups,
            List<AddressablesGroupData> groups)
        {
            foreach (var rawGroup in rawGroups)
            {
                if (rawGroup == null) continue;
                var t = rawGroup.GetType();
                var nameVal  = t.GetProperty("Name")?.GetValue(rawGroup)?.ToString() ?? "Unknown";
                var groupData = new AddressablesGroupData { Name = nameVal };

                var bundlesProp = t.GetProperty("Bundles") ?? t.GetProperty("Files");
                if (bundlesProp?.GetValue(rawGroup) is System.Collections.IList rawBundles)
                {
                    foreach (var rawBundle in rawBundles)
                    {
                        if (rawBundle == null) continue;
                        var bt = rawBundle.GetType();
                        var bundleData = new AddressablesBundleData
                        {
                            Name      = bt.GetProperty("Name")?.GetValue(rawBundle)?.ToString() ?? "Unknown",
                            GroupName = nameVal,
                            SizeBytes = GetLong(bt, rawBundle, "FileSize", "Size")
                        };

                        var assetsProp = bt.GetProperty("Assets") ?? bt.GetProperty("ExplicitAssets");
                        if (assetsProp?.GetValue(rawBundle) is System.Collections.IList rawAssets)
                        {
                            foreach (var rawAsset in rawAssets)
                            {
                                if (rawAsset == null) continue;
                                var at = rawAsset.GetType();
                                bundleData.Assets.Add(new AddressablesAssetData
                                {
                                    AssetPath  = at.GetProperty("AssetPath")?.GetValue(rawAsset)?.ToString() ?? "",
                                    AssetGuid  = at.GetProperty("Guid")?.GetValue(rawAsset)?.ToString() ?? "",
                                    SizeBytes  = GetLong(at, rawAsset, "SerializedSize", "Size"),
                                    BundleName = bundleData.Name,
                                    GroupName  = nameVal,
                                    IsImplicit = false
                                });
                            }
                        }

                        groupData.TotalSizeBytes += bundleData.SizeBytes;
                        groupData.Bundles.Add(bundleData);
                    }
                }

                groups.Add(groupData);
            }
        }

        private static void ParseDuplicationsFromReflection(System.Collections.IList rawDups,
            List<DuplicationData> duplications)
        {
            foreach (var rawDup in rawDups)
            {
                if (rawDup == null) continue;
                var t = rawDup.GetType();
                var dup = new DuplicationData
                {
                    AssetGuid  = t.GetProperty("AssetGuid")?.GetValue(rawDup)?.ToString() ?? "",
                    SizeBytes  = GetLong(t, rawDup, "SerializedSize", "Size")
                };
                var dupObjsProp = t.GetProperty("DuplicatedObjects");
                if (dupObjsProp?.GetValue(rawDup) is System.Collections.IList dupObjs)
                {
                    foreach (var dupObj in dupObjs)
                    {
                        if (dupObj == null) continue;
                        var dt = dupObj.GetType();
                        var bundleName = dt.GetProperty("Bundle")?.GetValue(dupObj)?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(bundleName) && !dup.PresentInBundles.Contains(bundleName))
                            dup.PresentInBundles.Add(bundleName);
                    }
                }
                dup.EstimatedWastedBytes = dup.SizeBytes * Mathf.Max(0, dup.PresentInBundles.Count - 1);
                duplications.Add(dup);
            }
        }

        /// <summary>
        /// Simple text-based fallback parser for plain-text buildlayout files.
        /// </summary>
        private static void ParseTextLayout(string layoutFile, List<AddressablesGroupData> groups)
        {
            // Minimal text parser – reads group/bundle/asset lines
            var lines = File.ReadAllLines(layoutFile);
            AddressablesGroupData  currentGroup  = null;
            AddressablesBundleData currentBundle = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimStart();
                if (line.StartsWith("Group:"))
                {
                    currentGroup  = new AddressablesGroupData { Name = AfterColon(line) };
                    currentBundle = null;
                    groups.Add(currentGroup);
                }
                else if (line.StartsWith("Bundle:") && currentGroup != null)
                {
                    currentBundle = new AddressablesBundleData
                    {
                        Name      = AfterColon(line),
                        GroupName = currentGroup.Name
                    };
                    currentGroup.Bundles.Add(currentBundle);
                }
                else if (line.StartsWith("- ") && currentBundle != null)
                {
                    // Asset line
                    currentBundle.Assets.Add(new AddressablesAssetData
                    {
                        AssetPath  = line.Substring(2).Trim(),
                        BundleName = currentBundle.Name,
                        GroupName  = currentBundle.GroupName
                    });
                }
                else if (line.StartsWith("FileSize:") && currentBundle != null)
                {
                    if (long.TryParse(AfterColon(line), out long sz))
                    {
                        currentBundle.SizeBytes = sz;
                        if (currentGroup != null) currentGroup.TotalSizeBytes += sz;
                    }
                }
            }
        }

        // ---- Helpers ----

        private static long GetLong(Type t, object obj, params string[] propNames)
        {
            foreach (var name in propNames)
            {
                var prop = t.GetProperty(name);
                if (prop != null)
                {
                    var val = prop.GetValue(obj);
                    if (val is ulong ul) return (long)ul;
                    if (val is long l)   return l;
                    if (val is int i)    return i;
                    if (long.TryParse(val?.ToString(), out long parsed)) return parsed;
                }
            }
            return 0;
        }

        private static string AfterColon(string line)
        {
            var idx = line.IndexOf(':');
            return idx >= 0 ? line.Substring(idx + 1).Trim() : line.Trim();
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }
    }
}
