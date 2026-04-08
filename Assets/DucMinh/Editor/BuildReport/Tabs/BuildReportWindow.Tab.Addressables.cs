using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private void BuildAddressablesTab(VisualElement container)
        {
            if (!EnsureReport(container)) return;

            var groups = _currentReport.AddressablesGroups;

            if (groups == null || groups.Count == 0)
            {
                container.CreateHelpBox(
                    "No Addressables build layout data found.\n" +
                    "Enable 'Debug Build Layout' in Addressables settings and rebuild content.",
                    HelpBoxMessageType.Info).SetMargin(12);
                return;
            }

            var (_, tabContent) = container.CreateEditorTabs(
                new[] { "Groups & Bundles", "All Assets", "Duplicates" },
                0,
                (idx, c) =>
                {
                    switch (idx)
                    {
                        case 0: BuildAddrGroupsView(c, groups);    break;
                        case 1: BuildAddrAssetsView(c, groups);    break;
                        case 2: BuildAddrDuplicatesView(c);        break;
                    }
                });
        }

        // ── Groups & Bundles ──────────────────────────────────────────────

        private void BuildAddrGroupsView(VisualElement c, System.Collections.Generic.List<AddressablesGroupData> groups)
        {
            // Summary cards
            var grid = c.CreateRow();
            grid.style.flexWrap = Wrap.Wrap;
            grid.SetPadding(4, 0);
            AddStatCard(grid, "Groups",  groups.Count.ToString());
            AddStatCard(grid, "Bundles", groups.Sum(g => g.Bundles.Count).ToString());
            AddStatCard(grid, "Total",   FormatBytes(_currentReport.AddressablesTotalBytes));

            var scroll = c.CreateScrollView();
            scroll.SetFlex(1);

            foreach (var group in groups.OrderByDescending(g => g.TotalSizeBytes))
            {
                // Group row
                var groupRow = scroll.CreateRow();
                groupRow.SetPadding(6, 3);
                groupRow.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
                groupRow.style.marginBottom = 1;
                groupRow.CreateLabel($"📦 {group.Name}").SetFlex(1)
                        .style.unityFontStyleAndWeight = FontStyle.Bold;
                groupRow.CreateLabel(FormatBytes(group.TotalSizeBytes)).SetWidth(90).SetFontSize(11);

                // Bundle rows
                foreach (var bundle in group.Bundles.OrderByDescending(b => b.SizeBytes))
                {
                    var bundleRow = scroll.CreateRow();
                    bundleRow.SetPaddingLeft(24).SetPaddingTop(2).SetPaddingBottom(2);
                    bundleRow.style.borderBottomWidth = 1;
                    bundleRow.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                    bundleRow.CreateLabel($"  {bundle.Name}").SetFlex(1).SetFontSize(11)
                             .SetTextColor(new Color(0.8f, 0.8f, 0.8f));
                    bundleRow.CreateLabel($"{bundle.Assets.Count} assets").SetWidth(80).SetFontSize(10)
                             .SetTextColor(new Color(0.6f, 0.6f, 0.6f));
                    bundleRow.CreateLabel(FormatBytes(bundle.SizeBytes)).SetWidth(90).SetFontSize(11);
                }
            }
        }

        // ── All Assets ────────────────────────────────────────────────────

        private void BuildAddrAssetsView(VisualElement c, System.Collections.Generic.List<AddressablesGroupData> groups)
        {
            var allAssets = groups
                .SelectMany(g => g.Bundles)
                .SelectMany(b => b.Assets)
                .OrderByDescending(a => a.SizeBytes)
                .ToList();

            // Header
            var header = c.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            header.CreateLabel("Asset Path").SetFlex(1).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Group").SetWidth(120).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Bundle").SetWidth(140).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Size").SetWidth(90).style.unityFontStyleAndWeight = FontStyle.Bold;

            var scroll = c.CreateScrollView();
            scroll.SetFlex(1);

            foreach (var asset in allAssets)
            {
                var row = scroll.CreateRow();
                row.SetPadding(4, 2);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                row.OnHover(h => row.style.backgroundColor = h ? new Color(0.28f, 0.28f, 0.28f) : StyleKeyword.Null);
                var capPath = asset.AssetPath;
                row.OnClick(() => PingAsset(capPath));

                row.CreateLabel(asset.AssetPath).SetFlex(1).SetFontSize(10);
                row.CreateLabel(asset.GroupName).SetWidth(120).SetFontSize(10)
                   .SetTextColor(new Color(0.7f, 0.7f, 0.7f));
                row.CreateLabel(asset.BundleName).SetWidth(140).SetFontSize(10)
                   .SetTextColor(new Color(0.65f, 0.65f, 0.65f));
                row.CreateLabel(asset.SizeBytes > 0 ? FormatBytes(asset.SizeBytes) : "-").SetWidth(90).SetFontSize(10);
            }

            c.CreateLabel($"  {allAssets.Count} assets").SetFontSize(10).SetPadding(4)
             .SetTextColor(new Color(0.55f, 0.55f, 0.55f));
        }

        // ── Duplicates ────────────────────────────────────────────────────

        private void BuildAddrDuplicatesView(VisualElement c)
        {
            var dups = _currentReport.Duplications;

            if (dups == null || dups.Count == 0)
            {
                c.CreateHelpBox("No duplicate assets detected.", HelpBoxMessageType.Info).SetMargin(12);
                return;
            }

            var header = c.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            header.CreateLabel("Asset").SetFlex(1).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("In Bundles").SetWidth(80).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Wasted").SetWidth(90).style.unityFontStyleAndWeight = FontStyle.Bold;

            var scroll = c.CreateScrollView();
            scroll.SetFlex(1);

            long totalWasted = 0;
            foreach (var dup in dups.OrderByDescending(d => d.EstimatedWastedBytes))
            {
                totalWasted += dup.EstimatedWastedBytes;
                var row = scroll.CreateRow();
                row.SetPadding(4, 2);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

                row.CreateLabel(string.IsNullOrEmpty(dup.AssetPath) ? dup.AssetGuid : dup.AssetPath)
                   .SetFlex(1).SetFontSize(10);
                row.CreateLabel(dup.PresentInBundles.Count.ToString()).SetWidth(80).SetFontSize(10);
                row.CreateLabel(FormatBytes(dup.EstimatedWastedBytes)).SetWidth(90).SetFontSize(10)
                   .SetTextColor(new Color(0.95f, 0.5f, 0.2f));
            }

            c.CreateLabel($"  {dups.Count} duplicates  |  Total wasted: {FormatBytes(totalWasted)}")
             .SetFontSize(10).SetPadding(4).SetTextColor(new Color(0.9f, 0.5f, 0.2f));
        }
    }
}
