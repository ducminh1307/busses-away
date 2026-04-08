using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using DucMinh;
using DucMinh.UIToolkit;

namespace DucMinh.BuildReport
{
    /// <summary>
    /// Main Build Report EditorWindow.
    /// Tabs are implemented in partial classes:
    ///   BuildReportWindow.Tab.Summary.cs
    ///   BuildReportWindow.Tab.Size.cs
    ///   BuildReportWindow.Tab.Time.cs
    ///   BuildReportWindow.Tab.Addressables.cs
    ///   BuildReportWindow.Tab.AssetsUsage.cs
    ///   BuildReportWindow.Tab.History.cs
    /// </summary>
    public partial class BuildReportWindow : EditorWindow
    {
        // ── State ─────────────────────────────────────────────────────────
        private CombinedBuildReport _currentReport;
        private int                 _activeTab = 0;

        private static readonly string[] TabLabels =
        {
            "Summary", "Size", "Time", "Addressables", "Assets Usage", "History"
        };

        // ── Menu ──────────────────────────────────────────────────────────
        [MenuItem("Window/DucMinh/Build Report", priority = 2000)]
        [MenuItem("Tools/Build/Open Build Report Window")]
        public static void Open()
        {
            var wnd = GetWindow<BuildReportWindow>();
            wnd.titleContent = new GUIContent("Build Report", EditorGUIUtility.IconContent("d_Search Icon").image);
            wnd.minSize = new Vector2(700, 480);
        }

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void CreateGUI()
        {
            ApplyRootStyle(rootVisualElement);

            // Load most recent report on open
            _currentReport = CombinedBuildReportStorage.LoadLatest();

            // Toolbar tabs
            var (_, content) = rootVisualElement.CreateEditorTabs(TabLabels, _activeTab, (index, container) =>
            {
                _activeTab = index;
                container.Clear();
                switch (index)
                {
                    case 0: BuildSummaryTab(container); break;
                    case 1: BuildSizeTab(container);    break;
                    case 2: BuildTimeTab(container);    break;
                    case 3: BuildAddressablesTab(container); break;
                    case 4: BuildAssetsUsageTab(container);  break;
                    case 5: BuildHistoryTab(container);      break;
                }
            });
        }

        // ── Helpers shared across tabs ────────────────────────────────────

        private static void ApplyRootStyle(VisualElement root)
        {
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow      = 1;
        }

        /// <summary>Shows a "No report data" placeholder when _currentReport is null.</summary>
        protected bool EnsureReport(VisualElement container)
        {
            if (_currentReport != null) return true;

            var box = container.CreateHelpBox(
                "No build report found.\n" +
                "Run a build via 'Tools → Build → Build With Report', or wait for any build to complete.",
                HelpBoxMessageType.Info);
            box.SetMargin(12);
            return false;
        }

        /// <summary>Formats bytes to human-readable string.</summary>
        protected static string FormatBytes(long bytes)
        {
            if (bytes <= 0)       return "0 B";
            if (bytes < 1024)     return $"{bytes} B";
            if (bytes < 1024*1024) return $"{bytes/1024.0:F1} KB";
            if (bytes < 1024L*1024*1024) return $"{bytes/1024.0/1024:F2} MB";
            return $"{bytes/1024.0/1024/1024:F2} GB";
        }

        /// <summary>Formats seconds to mm:ss or h:mm:ss.</summary>
        protected static string FormatTime(double seconds)
        {
            var ts = System.TimeSpan.FromSeconds(seconds);
            return ts.TotalHours >= 1
                ? $"{(int)ts.TotalHours}h {ts.Minutes:D2}m {ts.Seconds:D2}s"
                : $"{ts.Minutes:D2}m {ts.Seconds:D2}s";
        }

        /// <summary>Converts a Unix timestamp to local date-time string.</summary>
        protected static string FormatTimestamp(long utcUnix)
        {
            if (utcUnix == 0) return "-";
            var dt = System.DateTimeOffset.FromUnixTimeSeconds(utcUnix).LocalDateTime;
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>Creates a styled section header label.</summary>
        protected static Label CreateSectionHeader(VisualElement parent, string text)
        {
            var lbl = parent.CreateLabel(text);
            lbl.style.fontSize    = 12;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.SetPadding(8, 4);
            lbl.SetMarginTop(8);
            return lbl;
        }

        /// <summary>Creates a divider line.</summary>
        protected static void CreateDivider(VisualElement parent)
        {
            parent.CreateDivider(new Color(0.25f, 0.25f, 0.25f), 1f, 4f);
        }
    }
}
