using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace DucMinh.BuildReport
{
    public static class UsedAssetsCollector
    {
        /// <summary>
        /// Returns a HashSet of asset paths that are used in the given build report
        /// (sourced from both Unity's source assets and Addressables layout).
        /// </summary>
        public static HashSet<string> Collect(CombinedBuildReport report)
        {
            var used = new HashSet<string>();

            // From Unity BuildReport source assets
            if (report.SourceAssets != null)
                foreach (var a in report.SourceAssets)
                    if (!string.IsNullOrEmpty(a.Path)) used.Add(a.Path);

            // From Addressables
            if (report.AddressablesGroups != null)
                foreach (var g in report.AddressablesGroups)
                    foreach (var b in g.Bundles)
                        foreach (var a in b.Assets)
                            if (!string.IsNullOrEmpty(a.AssetPath)) used.Add(a.AssetPath);

            return used;
        }
    }

    public static class UnusedAssetsFinder
    {
        // Folders that are always excluded from "unused" analysis
        private static readonly string[] ExcludedFolders =
        {
            "Assets/Resources/",
            "Assets/StreamingAssets/",
            "Assets/Plugins/",
            "Assets/Editor/",
        };

        public static List<string> Find(CombinedBuildReport report)
        {
            var used   = UsedAssetsCollector.Collect(report);
            var unused = new List<string>();

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                // Only care about user assets
                if (!path.StartsWith("Assets/")) continue;

                // Skip excluded folders
                bool excluded = false;
                foreach (var folder in ExcludedFolders)
                {
                    if (path.StartsWith(folder)) { excluded = true; break; }
                }
                if (excluded) continue;

                // Skip folders themselves
                if (Directory.Exists(path)) continue;

                // Skip assets with an explicit AssetBundle name (managed separately)
                var importer = AssetImporter.GetAtPath(path);
                if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName)) continue;

                if (!used.Contains(path))
                    unused.Add(path);
            }

            return unused;
        }
    }
}
