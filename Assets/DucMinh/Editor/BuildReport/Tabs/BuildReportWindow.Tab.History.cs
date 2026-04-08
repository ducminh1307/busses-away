using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private List<BuildHistoryEntry> _historyEntries;
        private int _compareA = -1;
        private int _compareB = -1;

        private void BuildHistoryTab(VisualElement container)
        {
            _historyEntries = CombinedBuildReportStorage.LoadAllHistoryEntries()
                .OrderByDescending(e => e.TimestampUtc)
                .ToList();

            if (_historyEntries.Count == 0)
            {
                container.CreateHelpBox(
                    "No build history yet. Run a build first.",
                    HelpBoxMessageType.Info).SetMargin(12);
                return;
            }

            // ── Toolbar ────────────────────────────────────────────────────
            var toolbar = container.CreateRow();
            toolbar.SetPadding(6, 4);
            toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
            toolbar.style.alignItems = Align.Center;
            toolbar.CreateLabel($"{_historyEntries.Count} builds in history").SetFlex(1).SetFontSize(11)
                   .SetTextColor(new Color(0.7f, 0.7f, 0.7f));
            toolbar.CreateButton("Clear Selection", () =>
            {
                _compareA = _compareB = -1;
                RefreshHistoryComparison(container);
            }).SetWidth(120);

            // ── Column header ──────────────────────────────────────────────
            var header = container.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            AddHistoryColHeader(header, "Date",       0.22f);
            AddHistoryColHeader(header, "Platform",   0.12f);
            AddHistoryColHeader(header, "Result",     0.10f);
            AddHistoryColHeader(header, "Size",       0.14f);
            AddHistoryColHeader(header, "Time",       0.12f);
            AddHistoryColHeader(header, "Addr.",      0.12f);
            AddHistoryColHeader(header, "Err / Warn", 0.10f);
            AddHistoryColHeader(header, "Compare",    0.08f);

            // ── List ───────────────────────────────────────────────────────
            var scroll = container.CreateScrollView();
            scroll.SetFlex(1);

            for (int i = 0; i < _historyEntries.Count; i++)
            {
                int   captureI = i;
                var   entry    = _historyEntries[i];
                bool  isSelected = (i == _compareA || i == _compareB);

                var row = scroll.CreateRow();
                row.SetName($"history-row-{i}");
                row.SetPadding(4, 3);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                if (isSelected) row.style.backgroundColor = new Color(0.23f, 0.33f, 0.23f);

                row.OnHover(h => row.style.backgroundColor =
                    (captureI == _compareA || captureI == _compareB)
                        ? new Color(0.23f, 0.33f, 0.23f)
                        : (h ? new Color(0.28f, 0.28f, 0.28f) : StyleKeyword.Null));

                // Load full report on click
                row.OnClick(() =>
                {
                    var report = CombinedBuildReportStorage.Load(_historyEntries[captureI].ReportFolderPath);
                    if (report != null)
                    {
                        _currentReport = report;
                        Open(); // Refresh window
                    }
                });

                row.CreateLabel(FormatTimestamp(entry.TimestampUtc)).SetFlex(0.22f).SetFontSize(10);
                row.CreateLabel(entry.Platform ?? "-").SetFlex(0.12f).SetFontSize(10);

                var resultColor = entry.Result == "Succeeded"
                    ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.9f, 0.3f, 0.3f);
                row.CreateLabel(entry.Result ?? "-").SetFlex(0.10f).SetFontSize(10).SetTextColor(resultColor);

                row.CreateLabel(FormatBytes(entry.TotalSizeBytes)).SetFlex(0.14f).SetFontSize(10);
                row.CreateLabel(FormatTime(entry.TotalTimeSeconds)).SetFlex(0.12f).SetFontSize(10);
                row.CreateLabel(entry.AddressablesTotalBytes > 0
                    ? FormatBytes(entry.AddressablesTotalBytes) : "-").SetFlex(0.12f).SetFontSize(10);
                row.CreateLabel($"{entry.ErrorCount}/{entry.WarningCount}").SetFlex(0.10f).SetFontSize(10)
                   .SetTextColor(entry.ErrorCount > 0 ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.65f, 0.65f, 0.65f));

                // Compare toggle
                var cmpBtn = row.CreateButton(isSelected ? "✓" : "◯", () =>
                {
                    if (_compareA == captureI) _compareA = -1;
                    else if (_compareB == captureI) _compareB = -1;
                    else if (_compareA < 0) _compareA = captureI;
                    else if (_compareB < 0) _compareB = captureI;
                    RefreshHistoryComparison(container);
                });
                cmpBtn.SetFlex(0.08f);
                cmpBtn.style.backgroundColor = isSelected
                    ? new Color(0.2f, 0.6f, 0.3f) : StyleKeyword.Null;
            }

            // ── Comparison panel ───────────────────────────────────────────
            RefreshHistoryComparison(container);
        }

        private void RefreshHistoryComparison(VisualElement container)
        {
            var existing = container.Q("compare-panel");
            existing?.RemoveFromHierarchy();

            if (_compareA < 0 || _compareB < 0 ||
                _compareA >= _historyEntries.Count || _compareB >= _historyEntries.Count) return;

            var ea = _historyEntries[_compareA];
            var eb = _historyEntries[_compareB];

            var panel = container.Create<VisualElement>("compare-panel");
            panel.SetPadding(8);
            panel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            panel.style.borderTopWidth = 2;
            panel.style.borderTopColor = new Color(0.3f, 0.6f, 0.4f);

            panel.CreateLabel("Comparison").style.unityFontStyleAndWeight = FontStyle.Bold;

            var grid = panel.CreateRow();
            grid.style.flexWrap = Wrap.Wrap;
            grid.SetMarginTop(6);

            long   sizeDelta = ea.TotalSizeBytes - eb.TotalSizeBytes;
            double timeDelta = ea.TotalTimeSeconds - eb.TotalTimeSeconds;
            long   addrDelta = ea.AddressablesTotalBytes - eb.AddressablesTotalBytes;

            AddDeltaCard(grid, "Size Δ",  FormatBytes(System.Math.Abs(sizeDelta)), sizeDelta < 0);
            AddDeltaCard(grid, "Time Δ",  FormatTime(System.Math.Abs(timeDelta)), timeDelta < 0);
            if (ea.AddressablesTotalBytes > 0 || eb.AddressablesTotalBytes > 0)
                AddDeltaCard(grid, "Addr. Δ", FormatBytes(System.Math.Abs(addrDelta)), addrDelta < 0);
        }

        private static void AddHistoryColHeader(VisualElement parent, string text, float flex)
        {
            var lbl = parent.CreateLabel(text);
            lbl.SetFlex(flex);
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.SetFontSize(11);
        }

        private static void AddDeltaCard(VisualElement parent, string label, string value, bool positive)
        {
            var card = parent.Create<VisualElement>();
            card.SetSize(140, 56).SetMargin(4).SetPadding(8);
            card.style.backgroundColor = positive ? new Color(0.18f, 0.28f, 0.18f) : new Color(0.28f, 0.18f, 0.18f);
            card.SetBorderRadius(4);
            card.CreateLabel(label).SetFontSize(10).SetTextColor(new Color(0.65f, 0.65f, 0.65f));
            var v = card.CreateLabel((positive ? "▼ -" : "▲ +") + value).SetFontSize(12);
            v.style.unityFontStyleAndWeight = FontStyle.Bold;
            v.SetTextColor(positive ? new Color(0.3f, 0.85f, 0.4f) : new Color(0.85f, 0.35f, 0.3f));
        }
    }
}
