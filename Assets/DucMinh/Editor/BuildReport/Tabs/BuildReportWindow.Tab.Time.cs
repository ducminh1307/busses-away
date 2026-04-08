using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    public partial class BuildReportWindow
    {
        private void BuildTimeTab(VisualElement container)
        {
            if (!EnsureReport(container)) return;

            var steps = _currentReport.BuildSteps;
            if (steps == null || steps.Count == 0)
            {
                container.CreateHelpBox("No build step data available.", HelpBoxMessageType.Info).SetMargin(12);
                return;
            }

            double totalTime = _currentReport.Summary?.TotalTimeSeconds ?? steps.Sum(s => s.DurationSeconds);

            // ── Column header ──────────────────────────────────────────────
            var header = container.CreateRow();
            header.SetPadding(6, 3);
            header.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);
            header.CreateLabel("Build Step").SetFlex(1).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Duration").SetWidth(90).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("% Total").SetWidth(70).style.unityFontStyleAndWeight = FontStyle.Bold;
            header.CreateLabel("Bar").SetFlex(0.4f).style.unityFontStyleAndWeight = FontStyle.Bold;

            // ── Scrollable list ────────────────────────────────────────────
            var scroll = container.CreateScrollView();
            scroll.SetFlex(1);

            // Sort top-level steps by duration desc; indent children
            var sorted = steps.OrderByDescending(s => s.DurationSeconds).ToList();

            foreach (var step in sorted)
            {
                float pct = totalTime > 0 ? (float)(step.DurationSeconds / totalTime) : 0f;

                var row = scroll.CreateRow();
                row.SetPadding(4 + step.Depth * 12, 2);
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);
                row.OnHover(h => row.style.backgroundColor = h ? new Color(0.28f, 0.28f, 0.28f) : StyleKeyword.Null);

                row.CreateLabel(step.Name).SetFlex(1).SetFontSize(11);
                row.CreateLabel(FormatTime(step.DurationSeconds)).SetWidth(90).SetFontSize(11);
                row.CreateLabel($"{pct*100f:F1}%").SetWidth(70).SetFontSize(11)
                   .SetTextColor(pct > 0.2f ? new Color(0.9f, 0.6f, 0.2f) : new Color(0.65f, 0.65f, 0.65f));

                // Progress bar
                var barBg = row.Create<VisualElement>();
                barBg.SetFlex(0.4f).SetHeight(8).SetBorderRadius(3).SetMarginLeft(4);
                barBg.style.backgroundColor = new Color(0.28f, 0.28f, 0.28f);
                var bar = barBg.Create<VisualElement>().SetHeight(8).SetBorderRadius(3);
                bar.style.width = Length.Percent(pct * 100f);
                bar.style.backgroundColor = Color.Lerp(new Color(0.2f, 0.75f, 0.4f), new Color(0.9f, 0.35f, 0.2f), pct * 2.5f);
            }

            // Footer
            container.CreateLabel($"  {steps.Count} steps  |  Total: {FormatTime(totalTime)}")
                     .SetFontSize(10).SetPadding(4)
                     .SetTextColor(new Color(0.55f, 0.55f, 0.55f));
        }
    }
}
