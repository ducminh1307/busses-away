using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private string _sizeFilterText    = "";
        private string _sizeFilterExt     = "All";
        private List<SourceAssetData> _sortedAssets;

        private void BuildSizeTab(VisualElement container)
        {
            if (!EnsureReport(container)) return;

            // ── Toolbar ────────────────────────────────────────────────────
            var toolbar = container.CreateRow();
            toolbar.SetPadding(6, 4);
            toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            toolbar.style.alignItems = Align.Center;

            toolbar.CreateLabel("Filter: ").SetFontSize(11);
            var search = toolbar.CreateTextField("", _sizeFilterText);
            search.SetFlex(1);
            search.SetMargin(0, 4);
            search.RegisterValueChangedCallback(evt =>
            {
                _sizeFilterText = evt.newValue;
                RefreshSizeList(container);
            });

            var extensions = BuildExtensionList();
            var extDrop    = toolbar.CreateDropdown("Ext:", extensions, _sizeFilterExt);
            extDrop.SetWidth(100);
            extDrop.RegisterValueChangedCallback(evt =>
            {
                _sizeFilterExt = evt.newValue;
                RefreshSizeList(container);
            });

            // Placeholder for list (will be filled by RefreshSizeList)
            var listContainer = container.Create<VisualElement>("size-list");
            listContainer.SetFlex(1);
            RefreshSizeList(container);
        }

        private void RefreshSizeList(VisualElement container)
        {
            // Gather filtered data
            _sortedAssets = FilteredSourceAssets();

            var listContainer = container.Q("size-list");
            if (listContainer == null) return;
            listContainer.Clear();

            // Column header
            var header = listContainer.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);
            AddColumnHeader(header, "Asset Path",  true,  0.6f);
            AddColumnHeader(header, "Type",        false, 0.15f);
            AddColumnHeader(header, "Size",        false, 0.15f);

            // List
            var scrollList = listContainer.CreateScrollView();
            scrollList.SetFlex(1);

            long totalBytes = _sortedAssets.Sum(a => a.SizeBytes);

            foreach (var asset in _sortedAssets)
            {
                var row = scrollList.CreateRow();
                row.SetPadding(4, 2);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                row.OnHover(h => row.style.backgroundColor = h ? new Color(0.28f, 0.28f, 0.28f) : StyleKeyword.Null);

                // Click to ping in Project
                var capPath = asset.Path;
                row.OnClick(() => PingAsset(capPath));

                row.CreateLabel(asset.Path).SetFlex(0.6f).SetFontSize(11);
                row.CreateLabel(asset.AssetType).SetFlex(0.15f).SetFontSize(11)
                   .SetTextColor(new Color(0.65f, 0.65f, 0.65f));

                float pct = totalBytes > 0 ? (float)asset.SizeBytes / totalBytes : 0;
                var sizeRow = row.CreateRow().SetFlex(0.15f);
                sizeRow.style.alignItems = Align.Center;
                sizeRow.CreateLabel(FormatBytes(asset.SizeBytes)).SetFontSize(11);

                // Mini bar
                var bar = sizeRow.Create<VisualElement>();
                bar.SetMarginLeft(4).SetWidth(40).SetHeight(6).SetBorderRadius(2);
                bar.style.backgroundColor = new Color(0.25f, 0.55f, 0.85f);
                bar.style.width = Length.Percent(pct * 100f);
            }

            // Summary row
            listContainer.CreateLabel($"  {_sortedAssets.Count} assets  |  Total: {FormatBytes(totalBytes)}")
                         .SetFontSize(10).SetPadding(4)
                         .SetTextColor(new Color(0.55f, 0.55f, 0.55f));
        }

        private List<SourceAssetData> FilteredSourceAssets()
        {
            var src = _currentReport?.SourceAssets ?? new List<SourceAssetData>();
            return src
                .Where(a =>
                    (string.IsNullOrEmpty(_sizeFilterText) ||
                     a.Path.IndexOf(_sizeFilterText, System.StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (_sizeFilterExt == "All" ||
                     Path.GetExtension(a.Path).TrimStart('.').Equals(_sizeFilterExt, System.StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(a => a.SizeBytes)
                .ToList();
        }

        private List<string> BuildExtensionList()
        {
            var exts = new List<string> { "All" };
            if (_currentReport?.SourceAssets != null)
            {
                exts.AddRange(
                    _currentReport.SourceAssets
                        .Select(a => Path.GetExtension(a.Path).TrimStart('.'))
                        .Where(e => !string.IsNullOrEmpty(e))
                        .Distinct()
                        .OrderBy(e => e));
            }
            return exts;
        }

        private static void AddColumnHeader(VisualElement parent, string text, bool flexGrow, float flex)
        {
            var lbl = parent.CreateLabel(text);
            lbl.SetFlex(flex);
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.SetFontSize(11);
        }

        private static void PingAsset(string path)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }
    }
}
