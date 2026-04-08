using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private void BuildSummaryTab(VisualElement container)
        {
            var scroll = container.CreateScrollView();
            scroll.SetFlex(1);

            if (!EnsureReport(scroll)) return;

            var s = _currentReport.Summary;

            // ── Header row ────────────────────────────────────────────────
            var headerRow = scroll.CreateRow();
            headerRow.SetPadding(8);
            headerRow.SetMarginBottom(4);
            headerRow.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

            var resultColor = s.Result == "Succeeded" ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.9f, 0.3f, 0.3f);
            headerRow.CreateLabel($"● {s.Result}").SetTextColor(resultColor).SetFontSize(13);
            headerRow.CreateLabel($"  {s.Platform}  |  {FormatTimestamp(s.TimestampUtc)}")
                     .SetFlex(1).SetFontSize(12);

            // ── Stat cards ────────────────────────────────────────────────
            CreateSectionHeader(scroll, "Build Overview");
            CreateDivider(scroll);

            var grid = scroll.CreateRow();
            grid.style.flexWrap = Wrap.Wrap;
            grid.SetPadding(4, 0);

            AddStatCard(grid, "Total Size",   FormatBytes(s.TotalSizeBytes));
            AddStatCard(grid, "Build Time",   FormatTime(s.TotalTimeSeconds));
            AddStatCard(grid, "Platform",     s.Platform);
            AddStatCard(grid, "Build Target", s.BuildTarget);
            AddStatCard(grid, "Errors",       s.ErrorCount.ToString(),
                s.ErrorCount > 0 ? new Color(0.9f, 0.3f, 0.3f) : (Color?)null);
            AddStatCard(grid, "Warnings",     s.WarningCount.ToString(),
                s.WarningCount > 0 ? new Color(0.9f, 0.7f, 0.2f) : (Color?)null);

            if (_currentReport.AddressablesTotalBytes > 0)
                AddStatCard(grid, "Addressables", FormatBytes(_currentReport.AddressablesTotalBytes));

            // ── Size breakdown bar ────────────────────────────────────────
            if (_currentReport.SourceAssets?.Count > 0 && s.TotalSizeBytes > 0)
            {
                CreateSectionHeader(scroll, "Size Breakdown  (top asset categories)");
                CreateDivider(scroll);

                var byType = _currentReport.SourceAssets
                    .GroupBy(a => a.AssetType)
                    .OrderByDescending(g => g.Sum(a => a.SizeBytes))
                    .Take(8)
                    .ToList();

                foreach (var group in byType)
                {
                    long total = group.Sum(a => a.SizeBytes);
                    float pct  = (float)total / s.TotalSizeBytes;
                    AddProgressRow(scroll, group.Key, FormatBytes(total), pct);
                }
            }

            // ── Time breakdown bar ────────────────────────────────────────
            if (_currentReport.BuildSteps?.Count > 0)
            {
                CreateSectionHeader(scroll, "Build Time Breakdown  (top steps)");
                CreateDivider(scroll);

                var topSteps = _currentReport.BuildSteps
                    .Where(s2 => s2.Depth == 0)
                    .OrderByDescending(s2 => s2.DurationSeconds)
                    .Take(6)
                    .ToList();

                foreach (var step in topSteps)
                {
                    float pct = (float)(step.DurationSeconds / s.TotalTimeSeconds);
                    AddProgressRow(scroll, step.Name, FormatTime(step.DurationSeconds), pct);
                }
            }
        }

        // ── Internal helpers ──────────────────────────────────────────────

        private static void AddStatCard(VisualElement parent, string label, string value, Color? valueColor = null)
        {
            var card = parent.Create<VisualElement>();
            card.SetSize(160, 64);
            card.SetMargin(4);
            card.SetPadding(8);
            card.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f);
            card.SetBorderRadius(4);

            card.CreateLabel(label).SetFontSize(10).SetTextColor(new Color(0.65f, 0.65f, 0.65f));

            var valLabel = card.CreateLabel(value).SetFontSize(13);
            valLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            if (valueColor.HasValue) valLabel.SetTextColor(valueColor.Value);
        }

        private static void AddProgressRow(VisualElement parent, string label, string sizeStr, float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            var row = parent.CreateRow();
            row.SetPadding(4, 2);
            row.style.alignItems = Align.Center;

            row.CreateLabel(label).SetWidth(200).SetFontSize(11);
            row.CreateLabel(sizeStr).SetWidth(80).SetFontSize(11)
               .SetTextColor(new Color(0.75f, 0.75f, 0.75f));

            var barBg = row.Create<VisualElement>();
            barBg.SetFlex(1).SetHeight(10);
            barBg.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            barBg.SetBorderRadius(3);
            barBg.SetMarginLeft(4);

            var bar = barBg.Create<VisualElement>();
            bar.SetHeight(10).SetBorderRadius(3);
            bar.style.width = Length.Percent(fraction * 100f);
            bar.style.backgroundColor = Color.Lerp(new Color(0.2f, 0.6f, 0.9f), new Color(0.9f, 0.4f, 0.2f), fraction);
        }
    }
}
