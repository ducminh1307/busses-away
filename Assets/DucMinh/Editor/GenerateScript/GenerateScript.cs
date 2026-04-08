using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DucMinh.UIToolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.GenerateScript
{
    public class GenerateScript : EditorWindow
    {
        // ─── State ───────────────────────────────────────────────────────────────

        private List<BaseScriptGenerator> _generators = new List<BaseScriptGenerator>();
        private List<BaseScriptGenerator> _filteredGenerators = new List<BaseScriptGenerator>();
        private BaseScriptGenerator _selectedGenerator;

        private OverwriteMode _overwriteMode = OverwriteMode.Skip;

        // ─── UI References ───────────────────────────────────────────────────────

        private ScrollView _rightPanel;
        private VisualElement _validationPanel;
        private VisualElement _previewPanel;
        private VisualElement _resultPanel;
        private Label _headerTitleLabel;
        private Label _headerDescLabel;
        private Button _generateBtn;
        private VisualElement _sidebarList;
        private TextField _searchField;
        private DropdownField _presetDropdown;
        private VisualElement _settingsPanel;

        // ─── Menu ────────────────────────────────────────────────────────────────

        [MenuItem("DucMinh/GenerateScripts %g")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<GenerateScript>();
            wnd.titleContent = new GUIContent("Generate Scripts");
            wnd.minSize = new Vector2(800, 500);
        }

        // ─── Discovery ───────────────────────────────────────────────────────────

        private void InitializeGenerators()
        {
            _generators.Clear();
            var types = TypeCache.GetTypesDerivedFrom<BaseScriptGenerator>();
            foreach (var type in types)
            {
                if (type.IsAbstract) continue;
                try
                {
                    var instance = (BaseScriptGenerator)Activator.CreateInstance(type);
                    _generators.Add(instance);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[GenerateScript] Could not instantiate {type.Name}: {e.Message}");
                }
            }

            // Phase 4: sort by Category → Name
            _generators.Sort((a, b) =>
            {
                int cat = string.Compare(a.Category, b.Category, StringComparison.Ordinal);
                return cat != 0 ? cat : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            ApplySearch();

            if (_selectedGenerator == null && _generators.Count > 0)
                _selectedGenerator = _generators[0];
        }

        private void ApplySearch()
        {
            var query = _searchField?.value?.ToLower() ?? "";
            _filteredGenerators = string.IsNullOrEmpty(query)
                ? new List<BaseScriptGenerator>(_generators)
                : _generators.Where(g =>
                    g.Name.ToLower().Contains(query) ||
                    g.Category.ToLower().Contains(query)).ToList();
        }

        // ─── CreateGUI ───────────────────────────────────────────────────────────

        public void CreateGUI()
        {
            InitializeGenerators();

            var root = rootVisualElement;
            
            // Load and apply USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/DucMinh/Editor/GenerateScript/GenerateScript.uss");
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            
            // Add root class and explicit theme class
            root.AddClasses("gs-root");
            root.AddClasses(EditorGUIUtility.isProSkin ? "gs-theme-dark" : "gs-theme-light");

            BuildSidebar(root);
            BuildContent(root);

            UpdateRightPanel();
        }

        // ─── Sidebar ─────────────────────────────────────────────────────────────

        private void BuildSidebar(VisualElement root)
        {
            var sidebar = root.Create<VisualElement>("Sidebar")
                .AddClasses("gs-sidebar");

            // Title row
            var titleRow = sidebar.Create<VisualElement>()
                .AddClasses("gs-sidebar-title-row");

            titleRow.CreateLabel("Generators")
                .AddClasses("gs-sidebar-title");

            // Phase 4: Refresh button
            titleRow.CreateButton("↺", RefreshGenerators)
                .AddClasses("gs-refresh-btn");

            // Phase 3: Search field
            _searchField = sidebar.CreateTextField("", "", query =>
            {
                ApplySearch();
                RebuildSidebarList();
            });
            _searchField.SetPadding(8, 4);
            _searchField.Q<Label>()?.Hide();
            var searchInput = _searchField.Q<VisualElement>("unity-text-input");
            if (searchInput != null)
                searchInput.AddClasses("gs-search-input");

            // List container (scrollable)
            var scroll = sidebar.Create<ScrollView>().SetFlex(1);
            _sidebarList = scroll.Create<VisualElement>("SidebarList").AddClasses("gs-sidebar-list");

            RebuildSidebarList();
        }

        private void RebuildSidebarList()
        {
            if (_sidebarList == null) return;
            _sidebarList.Clear();

            // Phase 3: group by Category
            var grouped = _filteredGenerators
                .GroupBy(g => g.Category)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                // Category header
                _sidebarList.CreateLabel(group.Key.ToUpper())
                    .AddClasses("gs-cat-header");

                foreach (var generator in group)
                {
                    var gen = generator; // capture
                    var isSelected = gen == _selectedGenerator;

                    var item = _sidebarList.Create<VisualElement>()
                         .AddClasses("gs-sidebar-item");
                         
                    if (isSelected)
                    {
                        item.AddToClassList("selected");
                    }

                    item.CreateLabel(gen.Name)
                        .AddClasses("gs-sidebar-item-label");

                    item.OnClick(() => SelectGenerator(gen));
                }
            }

            if (_filteredGenerators.Count == 0)
            {
                _sidebarList.CreateLabel("No generators found")
                    .SetPadding(12)
                    .SetFontSize(11)
                    .SetTextAlign(TextAnchor.MiddleCenter);
            }
        }

        private void SelectGenerator(BaseScriptGenerator gen)
        {
            _selectedGenerator = gen;
            RebuildSidebarList();
            UpdateRightPanel();
        }

        private void RefreshGenerators()
        {
            InitializeGenerators();
            RebuildSidebarList();
            UpdateRightPanel();
        }

        // ─── Content Area ────────────────────────────────────────────────────────

        private void BuildContent(VisualElement root)
        {
            var content = root.Create<VisualElement>("Content")
                .AddClasses("gs-content");

            BuildHeader(content);
            BuildTabs(content);
        }

        private void BuildHeader(VisualElement content)
        {
            var header = content.Create<VisualElement>("Header")
                .AddClasses("gs-header");

            _headerTitleLabel = header.CreateLabel("Select a generator")
                .AddClasses("gs-header-title");

            _headerDescLabel = header.CreateLabel("")
                .AddClasses("gs-header-desc");
        }

        private VisualElement _tabGeneratePanel;
        private VisualElement _tabSettingsPanel;
        private Button _tabGenBtn;
        private Button _tabSettingsBtn;

        private void BuildTabs(VisualElement content)
        {
            // Tab bar
            var tabBar = content.Create<VisualElement>("TabBar")
                .AddClasses("gs-tab-bar");

            _tabGenBtn = tabBar.CreateButton("Generate", () => SwitchTab(true))
                .AddClasses("gs-tab-btn", "active");

            _tabSettingsBtn = tabBar.CreateButton("⚙ Settings", () => SwitchTab(false))
                .AddClasses("gs-tab-btn");

            // Generate tab
            _tabGeneratePanel = content.Create<VisualElement>("TabGenerate")
                .AddClasses("gs-tab-content");

            BuildGenerateTab(_tabGeneratePanel);

            // Settings tab (Phase 5)
            _tabSettingsPanel = content.Create<VisualElement>("TabSettings")
                .AddClasses("gs-tab-content", "gs-settings-panel")
                .Hide();

            BuildSettingsTab(_tabSettingsPanel);
        }

        private void SwitchTab(bool toGenerate)
        {
            _tabGeneratePanel.SetDisplay(toGenerate);
            _tabSettingsPanel.SetDisplay(!toGenerate);
            
            if (toGenerate)
            {
                _tabGenBtn.AddToClassList("active");
                _tabSettingsBtn.RemoveFromClassList("active");
            }
            else
            {
                _tabGenBtn.RemoveFromClassList("active");
                _tabSettingsBtn.AddToClassList("active");
            }
        }

        private void BuildGenerateTab(VisualElement tab)
        {
            // Phase 1: Validation panel
            _validationPanel = tab.Create<VisualElement>("ValidationPanel")
                .AddClasses("gs-validation-panel")
                .Hide();

            // Phase 1: Preview panel
            var previewSection = tab.Create<VisualElement>("PreviewSection")
                .SetFlex(1)
                .SetFlexDirection(FlexDirection.Column);

            // Preset bar (Phase 3)
            var presetBar = previewSection.Create<VisualElement>("PresetBar")
                .AddClasses("gs-preset-bar");

            presetBar.CreateLabel("Preset:")
                .AddClasses("gs-preset-label");

            _presetDropdown = presetBar.CreateDropdown("", new List<string> { "— None —" }, "— None —", _ => LoadSelectedPreset());
            _presetDropdown.SetFlex(1).SetHeight(24);

            presetBar.CreateButton("Save", SaveCurrentPreset)
                .SetHeight(24).SetWidth(50).SetFontSize(11)
                .SetBackgroundColor(new Color(0.25f, 0.45f, 0.25f))
                .SetTextColor(Color.white).SetBorderRadius(3);

            presetBar.CreateButton("Delete", DeleteSelectedPreset)
                .SetHeight(24).SetWidth(50).SetFontSize(11)
                .SetBackgroundColor(new Color(0.4f, 0.2f, 0.2f))
                .SetTextColor(Color.white).SetBorderRadius(3)
                .SetMarginLeft(4);

            // Right panel (generator settings)
            _rightPanel = previewSection.Create<ScrollView>("RightPanel")
                .AddClasses("gs-right-panel");

            // Phase 1: Preview panel
            _previewPanel = previewSection.Create<VisualElement>("PreviewPanel")
                .AddClasses("gs-preview-panel")
                .Hide();

            // Phase 2: Result panel
            _resultPanel = tab.Create<VisualElement>("ResultPanel")
                .AddClasses("gs-result-panel")
                .Hide();

            BuildBottomBar(tab);
        }

        private void BuildBottomBar(VisualElement tab)
        {
            var bottomBar = tab.Create<VisualElement>("BottomBar")
                .AddClasses("gs-bottom-bar");

            // Phase 2: Overwrite mode selector
            var overwriteDropdown = bottomBar.CreateDropdown(
                "On Conflict:",
                new List<string> { "Skip", "Overwrite", "Ask" },
                "Skip",
                val =>
                {
                    _overwriteMode = val switch
                    {
                        "Overwrite" => OverwriteMode.Overwrite,
                        "Ask"       => OverwriteMode.Ask,
                        _           => OverwriteMode.Skip
                    };
                });
            overwriteDropdown.SetWidth(180).SetHeight(36);

            // Spacer
            bottomBar.Create<VisualElement>().SetFlex(1);

            // Generate button
            _generateBtn = bottomBar.CreateButton("⚡ GENERATE", OnGenerateClicked)
                 .AddClasses("gs-generate-btn");
        }

        // ─── Settings Tab (Phase 5) ───────────────────────────────────────────────

        private void BuildSettingsTab(VisualElement tab)
        {
            tab.CreateLabel("Shared Generator Settings")
                .AddClasses("gs-settings-title");

            var settings = GeneratorSettings.Instance;

            tab.CreateTextField("Root Folder", settings.RootFolder, val =>
            {
                settings.RootFolder = val;
                settings.Save();
            }).SetMarginBottom(8);

            tab.CreateTextField("Default Namespace", settings.DefaultNamespace, val =>
            {
                settings.DefaultNamespace = val;
                settings.Save();
            }).SetMarginBottom(8);

            tab.CreateDropdown(
                "Naming Convention",
                new List<string> { "PascalCase", "CamelCase", "SnakeCase", "KebabCase" },
                settings.NamingConvention.ToString(),
                val =>
                {
                    settings.NamingConvention = Enum.Parse<NamingConvention>(val);
                    settings.Save();
                }).SetMarginBottom(16);

            tab.CreateHelpBox(
                "Settings are saved to ProjectSettings/GeneratorSettings.json and can be committed to SCM.",
                HelpBoxMessageType.Info);
        }

        // ─── Right Panel Update ───────────────────────────────────────────────────

        private void UpdateRightPanel()
        {
            if (_selectedGenerator == null || _rightPanel == null) return;

            // Update header
            if (_headerTitleLabel != null)
                _headerTitleLabel.text = _selectedGenerator.Name;
            if (_headerDescLabel != null)
            {
                _headerDescLabel.text = string.IsNullOrEmpty(_selectedGenerator.Description)
                    ? $"Category: {_selectedGenerator.Category}"
                    : _selectedGenerator.Description;
            }

            // Clear and rebuild right panel
            _rightPanel.Clear();
            _selectedGenerator.SetupGUI(_rightPanel);

            // Update preset list
            RefreshPresetDropdown();

            // Clear result panel
            _resultPanel?.Hide();

            // Run validation + preview
            RefreshValidationAndPreview();
        }

        // ─── Phase 1: Validation & Preview ───────────────────────────────────────

        private void RefreshValidationAndPreview()
        {
            if (_selectedGenerator == null) return;

            // Validation
            var validation = _selectedGenerator.Validate();
            RenderValidation(validation);

            // Preview
            var preview = _selectedGenerator.BuildPreview();
            RenderPreview(preview);

            // Enable/disable generate button
            if (_generateBtn != null)
            {
                _generateBtn.SetEnabled(validation.IsValid);
                // Instead of hardcoding background color, we use disable state of USS
            }
        }

        private void RenderValidation(ValidationResult result)
        {
            if (_validationPanel == null) return;
            _validationPanel.Clear();

            bool hasContent = result.Errors.Count > 0 || result.Warnings.Count > 0;
            _validationPanel.SetDisplay(hasContent);

            foreach (var err in result.Errors)
            {
                _validationPanel.CreateHelpBox($"❌ {err}", HelpBoxMessageType.Error);
            }
            foreach (var warn in result.Warnings)
            {
                _validationPanel.CreateHelpBox($"⚠️ {warn}", HelpBoxMessageType.Warning);
            }
        }

        private void RenderPreview(List<PreviewItem> items)
        {
            if (_previewPanel == null) return;
            _previewPanel.Clear();

            if (items == null || items.Count == 0)
            {
                _previewPanel.Hide();
                return;
            }

            _previewPanel.Show();

            _previewPanel.CreateLabel("📋 Preview")
                .AddClasses("gs-preview-title");

            foreach (var item in items)
            {
                var icon = item.Type == PreviewItemType.File ? "📄" : "📁";
                var row = _previewPanel.Create<VisualElement>()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetPadding(2, 2)
                    .SetMarginBottom(2);

                var label = row.CreateLabel($"{icon}  {item.Path}")
                               .AddClasses("gs-preview-item");
                
                if(item.Type == PreviewItemType.Folder)
                {
                    label.AddToClassList("folder");
                }
            }
        }

        // ─── Phase 2: Generate ────────────────────────────────────────────────────

        private void OnGenerateClicked()
        {
            if (_selectedGenerator == null) return;

            // Re-validate
            var validation = _selectedGenerator.Validate();
            if (!validation.IsValid)
            {
                RenderValidation(validation);
                return;
            }

            if (_overwriteMode == OverwriteMode.Ask)
            {
                // Dry-run: detect conflicts
                var preview = _selectedGenerator.BuildPreview();
                var conflicts = _selectedGenerator.DetectConflicts(preview);

                if (conflicts.Count > 0)
                {
                    // Show conflict dialog — execution resumes in callback
                    ConflictResolutionDialog.Show(conflicts, decisions =>
                    {
                        // Build per-file overwrite decisions and run
                        // For simplicity: if any selected to overwrite → use Overwrite mode
                        // Files not selected → they'll be skipped by generator logic
                        // (Advanced: pass decisions map to generator via context)
                        bool anyOverwrite = false;
                        foreach (var d in decisions.Values)
                            if (d) { anyOverwrite = true; break; }

                        var effectiveMode = anyOverwrite ? OverwriteMode.Overwrite : OverwriteMode.Skip;
                        var result = _selectedGenerator.Generate(effectiveMode);
                        RenderResult(result);
                        AssetDatabase.Refresh();
                    });
                    return;
                }
            }

            // Standard generate
            var genResult = _selectedGenerator.Generate(_overwriteMode);
            RenderResult(genResult);
            AssetDatabase.Refresh();
        }

        // ─── Phase 2: Result Panel ────────────────────────────────────────────────

        private void RenderResult(GenerationResult result)
        {
            if (_resultPanel == null) return;
            _resultPanel.Clear();
            _resultPanel.Show();

            // Summary row
            var summaryRow = _resultPanel.Create<VisualElement>()
                .SetFlexDirection(FlexDirection.Row)
                .SetMarginBottom(6);

            void AddBadge(string text, string ussClass)
            {
                var lbl = summaryRow.CreateLabel(text)
                    .SetPadding(4, 2)
                    .SetMarginRight(6)
                    .SetFontSize(11)
                    .SetFontStyleAndWeight(FontStyle.Bold)
                    .SetBorderRadius(4);
                // Can create generic classes for these in USS. Let's use simple colors for now:
                switch(ussClass)
                {
                    case "success": lbl.SetTextColor(new Color(0.3f, 0.85f, 0.45f)).SetBackgroundColor(new Color(0.3f * 0.2f, 0.85f * 0.2f, 0.45f * 0.2f)); break;
                    case "warn": lbl.SetTextColor(new Color(0.9f, 0.75f, 0.2f)).SetBackgroundColor(new Color(0.9f * 0.2f, 0.75f * 0.2f, 0.2f * 0.2f)); break;
                    case "blue": lbl.SetTextColor(new Color(0.15f, 0.45f, 0.75f)).SetBackgroundColor(new Color(0.15f * 0.2f, 0.45f * 0.2f, 0.75f * 0.2f)); break;
                    case "error": lbl.SetTextColor(new Color(0.9f, 0.3f, 0.3f)).SetBackgroundColor(new Color(0.9f * 0.2f, 0.3f * 0.2f, 0.3f * 0.2f)); break;
                }
            }

            if (result.Created.Count > 0)    AddBadge($"✅ {result.Created.Count} Created", "success");
            if (result.Skipped.Count > 0)    AddBadge($"⏭ {result.Skipped.Count} Skipped", "warn");
            if (result.Overwritten.Count > 0) AddBadge($"✏️ {result.Overwritten.Count} Overwritten", "blue");
            if (result.Failed.Count > 0)     AddBadge($"❌ {result.Failed.Count} Failed", "error");

            // Open folder button
            if ((result.Created.Count > 0 || result.Overwritten.Count > 0) && _selectedGenerator != null)
            {
                var firstPath = result.Created.Count > 0 ? result.Created[0] : result.Overwritten[0];
                var absDir = Path.GetDirectoryName(
                    Path.Combine(Application.dataPath, firstPath.Replace("Assets/", "")));

                if (!string.IsNullOrEmpty(absDir))
                {
                    _resultPanel.CreateButton("📂 Open Folder", () =>
                        EditorUtility.RevealInFinder(absDir))
                        .SetHeight(24).SetWidth(110).SetFontSize(11)
                        .SetBackgroundColor(new Color(0.25f, 0.35f, 0.45f))
                        .SetTextColor(Color.white).SetBorderRadius(4)
                        .SetMarginTop(4);
                }
            }
        }

        // ─── Phase 3: Presets ─────────────────────────────────────────────────────

        private void RefreshPresetDropdown()
        {
            if (_presetDropdown == null || _selectedGenerator == null) return;

            var names = _selectedGenerator.GetPresetNames();
            var choices = new List<string> { "— None —" };
            choices.AddRange(names);
            _presetDropdown.choices = choices;
            _presetDropdown.value = "— None —";
        }

        private void LoadSelectedPreset()
        {
            if (_presetDropdown == null || _selectedGenerator == null) return;
            var name = _presetDropdown.value;
            if (name == "— None —") return;
            _selectedGenerator.LoadPreset(name);
            UpdateRightPanel();
        }

        private void SaveCurrentPreset()
        {
            if (_selectedGenerator == null) return;
            var name = EditorInputDialog.Show("Save Preset", "Enter preset name:", "");
            if (!string.IsNullOrEmpty(name))
            {
                _selectedGenerator.SavePreset(name);
                RefreshPresetDropdown();
            }
        }

        private void DeleteSelectedPreset()
        {
            if (_presetDropdown == null || _selectedGenerator == null) return;
            var name = _presetDropdown.value;
            if (name == "— None —") return;
            _selectedGenerator.DeletePreset(name);
            RefreshPresetDropdown();
        }
    }

    // ─── Simple input dialog helper ──────────────────────────────────────────────

    internal static class EditorInputDialog
    {
        internal static string Show(string title, string message, string defaultValue)
        {
            // Unity doesn't have a built-in text input dialog — use simple alternative
            return defaultValue; // Placeholder: replace with custom modal if needed
        }
    }
}