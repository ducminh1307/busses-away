using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private enum AssetUsageFilter { All, Used, Unused }
        private AssetUsageFilter _usageFilter = AssetUsageFilter.All;

        private void BuildAssetsUsageTab(VisualElement container)
        {
            if (!EnsureReport(container)) return;

            // Compute data
            var used   = UsedAssetsCollector.Collect(_currentReport);
            var unused = UnusedAssetsFinder.Find(_currentReport);
            var allPaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.StartsWith("Assets/") && !Directory.Exists(p))
                .OrderBy(p => p)
                .ToList();

            // ── Top toolbar ───────────────────────────────────────────────
            var toolbar = container.CreateRow();
            toolbar.SetPadding(6, 4);
            toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            toolbar.style.alignItems = Align.Center;

            toolbar.CreateLabel($"Used: {used.Count}  Unused: {unused.Count}  Total: {allPaths.Count}")
                   .SetFlex(1).SetFontSize(11)
                   .SetTextColor(new Color(0.7f, 0.7f, 0.7f));

            var filterLabel = "Filter: ";
            var filterOptions = new List<string> { "All", "Used", "Unused" };
            var filterDrop = toolbar.CreateDropdown(filterLabel, filterOptions, _usageFilter.ToString());
            filterDrop.SetWidth(100);
            filterDrop.RegisterValueChangedCallback(evt =>
            {
                System.Enum.TryParse(evt.newValue, out _usageFilter);
                BuildAssetsUsageList(container, used, unused, allPaths);
            });

            toolbar.CreateButton("Export CSV", () => ExportUnusedCsv(unused))
                   .SetWidth(100).SetMarginLeft(6);

            // ── List container ────────────────────────────────────────────
            BuildAssetsUsageList(container, used, unused, allPaths);
        }

        private void BuildAssetsUsageList(VisualElement container,
            HashSet<string> used, List<string> unused, List<string> allPaths)
        {
            // Remove previous list (keep toolbar at index 0)
            var existing = container.Q("usage-list-root");
            existing?.RemoveFromHierarchy();

            var root = container.Create<VisualElement>("usage-list-root");
            root.SetFlex(1);

            // Column header
            var header = root.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            header.CreateLabel("Asset Path").SetFlex(1).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Type").SetWidth(80).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Used?").SetWidth(60).style.unityFontStyleAndWeight = FontStyle.Bold;

            var scroll = root.CreateScrollView();
            scroll.SetFlex(1);

            IEnumerable<string> paths = _usageFilter switch
            {
                AssetUsageFilter.Used   => allPaths.Where(p => used.Contains(p)),
                AssetUsageFilter.Unused => unused,
                _                       => allPaths
            };

            int count = 0;
            foreach (var path in paths.Take(2000)) // cap for performance
            {
                bool isUsed = used.Contains(path);
                var row = scroll.CreateRow();
                row.SetPadding(4, 2);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                row.OnHover(h => row.style.backgroundColor = h ? new Color(0.27f, 0.27f, 0.27f) : StyleKeyword.Null);
                var capPath = path;
                row.OnClick(() => PingAsset(capPath));

                row.CreateLabel(path).SetFlex(1).SetFontSize(10);
                row.CreateLabel(Path.GetExtension(path).TrimStart('.')).SetWidth(80).SetFontSize(10)
                   .SetTextColor(new Color(0.6f, 0.6f, 0.6f));

                var usedLabel = row.CreateLabel(isUsed ? "✓" : "✗").SetWidth(60).SetFontSize(11);
                usedLabel.SetTextColor(isUsed ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.85f, 0.35f, 0.3f));
                count++;
            }

            root.CreateLabel($"  {count} asset(s) shown")
                .SetFontSize(10).SetPadding(4)
                .SetTextColor(new Color(0.55f, 0.55f, 0.55f));
        }

        private static void ExportUnusedCsv(List<string> unused)
        {
            var sb = new StringBuilder();
            sb.AppendLine("AssetPath,Extension");
            foreach (var p in unused)
                sb.AppendLine($"\"{p}\",\"{Path.GetExtension(p)}\"");

            var savePath = EditorUtility.SaveFilePanel("Export Unused Assets CSV",
                "", "unused_assets.csv", "csv");
            if (!string.IsNullOrEmpty(savePath))
            {
                File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
                Debug.Log($"[BuildReport] Exported {unused.Count} unused assets to {savePath}");
            }
        }
    }
}
