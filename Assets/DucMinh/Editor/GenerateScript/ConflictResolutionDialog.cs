using System;
using System.Collections.Generic;
using DucMinh.UIToolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.GenerateScript
{
    /// <summary>
    /// Dialog tổng hợp tất cả file conflicts khi OverwriteMode = Ask.
    /// User có thể chọn từng file, Overwrite All, hoặc Skip All.
    /// </summary>
    public class ConflictResolutionDialog : EditorWindow
    {
        private List<string> _conflicts;
        private Dictionary<string, bool> _overwriteDecisions = new Dictionary<string, bool>();
        private Action<Dictionary<string, bool>> _onConfirm;

        public static void Show(List<string> conflicts, Action<Dictionary<string, bool>> onConfirm)
        {
            var window = CreateInstance<ConflictResolutionDialog>();
            window.titleContent = new GUIContent("Conflict Resolution");
            window.minSize = new Vector2(520, 400);
            window.maxSize = new Vector2(800, 600);
            window._conflicts = conflicts;
            window._onConfirm = onConfirm;

            // Default: skip all
            foreach (var c in conflicts)
                window._overwriteDecisions[c] = false;

            window.ShowModal();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.SetPadding(0);
            root.SetFlexDirection(FlexDirection.Column);
            root.SetBackgroundColor(new Color(0.18f, 0.18f, 0.18f));

            // Header
            var header = root.Create<VisualElement>("Header")
                .SetPadding(16, 12)
                .SetBackgroundColor(new Color(0.13f, 0.13f, 0.13f));

            header.CreateLabel("⚠️ File Conflicts Detected")
                .SetFontSize(15)
                .SetFontStyleAndWeight(FontStyle.Bold)
                .SetTextColor(new Color(1f, 0.8f, 0.2f));

            header.CreateLabel($"The following {_conflicts.Count} file(s) already exist. Choose how to handle them:")
                .SetFontSize(11)
                .SetTextColor(new Color(0.7f, 0.7f, 0.7f))
                .SetMarginTop(4);

            // Quick action buttons row
            var quickActions = root.Create<VisualElement>("QuickActions")
                .SetFlexDirection(FlexDirection.Row)
                .SetPadding(10, 8)
                .SetBackgroundColor(new Color(0.15f, 0.15f, 0.15f));

            quickActions.CreateButton("✅ Overwrite All", () =>
            {
                foreach (var key in new List<string>(_overwriteDecisions.Keys))
                    _overwriteDecisions[key] = true;
                RefreshList(listContainer);
            }).SetHeight(28).SetFontSize(12)
              .SetBackgroundColor(new Color(0.2f, 0.5f, 0.2f))
              .SetTextColor(Color.white)
              .SetBorderRadius(4)
              .SetPadding(0, 6);

            var spacer = quickActions.Create<VisualElement>().SetWidth(8);

            quickActions.CreateButton("⏭ Skip All", () =>
            {
                foreach (var key in new List<string>(_overwriteDecisions.Keys))
                    _overwriteDecisions[key] = false;
                RefreshList(listContainer);
            }).SetHeight(28).SetFontSize(12)
              .SetBackgroundColor(new Color(0.4f, 0.35f, 0.1f))
              .SetTextColor(Color.white)
              .SetBorderRadius(4)
              .SetPadding(0, 6);

            // Scrollable conflict list
            var scroll = root.Create<ScrollView>("ScrollArea").SetFlex(1).SetPadding(12, 8);
            listContainer = scroll.Create<VisualElement>("ListContainer").SetFlex(1);
            RefreshList(listContainer);

            // Bottom bar
            var bottomBar = root.Create<VisualElement>("BottomBar")
                .SetFlexDirection(FlexDirection.Row)
                .SetPadding(12, 10)
                .SetJustifyContent(Justify.FlexEnd)
                .SetBorderWidth(1, 0, 0, 0)
                .SetBorderColor(new Color(0.25f, 0.25f, 0.25f))
                .SetBackgroundColor(new Color(0.13f, 0.13f, 0.13f));

            bottomBar.CreateButton("Cancel", () =>
            {
                Close();
            }).SetHeight(32).SetWidth(90).SetFontSize(12)
              .SetBackgroundColor(new Color(0.3f, 0.3f, 0.3f))
              .SetTextColor(Color.white)
              .SetBorderRadius(4);

            var spacer2 = bottomBar.Create<VisualElement>().SetWidth(10);

            bottomBar.CreateButton("Confirm", () =>
            {
                _onConfirm?.Invoke(_overwriteDecisions);
                Close();
            }).SetHeight(32).SetWidth(100).SetFontSize(12)
              .SetFontStyleAndWeight(FontStyle.Bold)
              .SetBackgroundColor(new Color(0.15f, 0.5f, 0.8f))
              .SetTextColor(Color.white)
              .SetBorderRadius(4);
        }

        private VisualElement listContainer;

        private void RefreshList(VisualElement container)
        {
            container.Clear();

            foreach (var conflict in _conflicts)
            {
                var row = container.Create<VisualElement>($"row_{conflict}")
                    .SetFlexDirection(FlexDirection.Row)
                    .SetPadding(8, 6)
                    .SetMarginBottom(4)
                    .SetBorderRadius(4)
                    .SetBackgroundColor(new Color(0.22f, 0.22f, 0.22f));

                bool willOverwrite = _overwriteDecisions.TryGetValue(conflict, out var v) && v;

                // Toggle overwrite
                var toggle = row.CreateToggle("", willOverwrite, newVal =>
                {
                    _overwriteDecisions[conflict] = newVal;
                });
                toggle.SetWidth(24);

                // Status icon + path
                var statusLabel = row.CreateLabel(willOverwrite ? "✏️ Overwrite" : "⏭ Skip")
                    .SetWidth(90)
                    .SetFontSize(11)
                    .SetTextColor(willOverwrite ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.8f, 0.7f, 0.3f));

                var pathLabel = row.CreateLabel(conflict)
                    .SetFlex(1)
                    .SetFontSize(11)
                    .SetTextColor(new Color(0.75f, 0.75f, 0.75f));

                // Re-render on toggle
                toggle.RegisterValueChangedCallback(evt =>
                {
                    _overwriteDecisions[conflict] = evt.newValue;
                    statusLabel.text = evt.newValue ? "✏️ Overwrite" : "⏭ Skip";
                    statusLabel.SetTextColor(evt.newValue
                        ? new Color(0.4f, 0.9f, 0.4f)
                        : new Color(0.8f, 0.7f, 0.3f));
                });
            }
        }
    }
}
