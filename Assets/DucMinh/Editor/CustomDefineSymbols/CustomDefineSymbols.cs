using System;
using System.Collections.Generic;
using System.Linq;
using DucMinh.UIToolkit;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh
{
    public class CustomDefineSymbols : EditorWindow
    {
        private const string CUSTOM_SYMBOLS_KEY = "DucMinh_Custom_Define_Symbols";
        private List<string> symbols;
        private Dictionary<string, bool> symbolStates;
        private string newSymbolName = "";
        private DefineSymbolConfig projectConfig;

        private VisualElement symbolListContainer;
        private ScrollView scrollView;
        private TextField newSymbolField;

        [MenuItem("DucMinh/Custom Define Symbols %t")]
        public static void OpenWindow()
        {
            var window = GetWindow<CustomDefineSymbols>("Custom Define Symbols");
            window.minSize = new Vector2(350, 450);
            window.Show();
        }

        public void CreateGUI()
        {
            LoadSymbols();
            RefreshSymbolStates();

            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            // 1. Config Management UI
            var configBox = root.Create<VisualElement>("ConfigBox", "help-box");
            configBox.style.flexDirection = FlexDirection.Row;
            configBox.style.paddingBottom = 5;
            configBox.style.marginBottom = 5;
            configBox.style.borderBottomWidth = 1;
            configBox.style.borderBottomColor = Color.gray;

            if (projectConfig == null)
            {
                configBox.CreateLabel("Shared config not found.");
                configBox.CreateButton("Create Config", CreateConfigAsset).SetFlex(0).style.width = 100;
            }
            else
            {
                configBox.CreateLabel("Shared Config: Loaded").SetFlex(1);
                configBox.CreateButton("Open", () =>
                {
                    Selection.activeObject = projectConfig;
                    EditorGUIUtility.PingObject(projectConfig);
                }).style.width = 60;
                
                configBox.CreateButton("Reload", () =>
                {
                    LoadSymbols();
                    RefreshSymbolStates();
                    RefreshUI();
                }).style.width = 60;
            }

            // 2. Symbols List
            root.CreateLabel("Select symbols to define:").style.unityFontStyleAndWeight = FontStyle.Bold;
            
            scrollView = root.Create<ScrollView>();
            scrollView.style.height = 300;
            scrollView.style.marginTop = 5;
            scrollView.style.marginBottom = 5;
            scrollView.SetBorderWidth(1);
            scrollView.SetBorderColor(Color.gray);

            symbolListContainer = scrollView.contentContainer;
            RefreshUI();

            // 3. Add New Symbol (Local)
            root.CreateLabel("Add New Symbol (Local):").style.unityFontStyleAndWeight = FontStyle.Bold;
            var addBox = root.Create<VisualElement>();
            addBox.style.flexDirection = FlexDirection.Row;
            
            newSymbolField = addBox.Create<TextField>();
            newSymbolField.SetFlex(1);
            newSymbolField.RegisterValueChangedCallback(evt => newSymbolName = evt.newValue);
            
            addBox.CreateButton("Add", () =>
            {
                if (!string.IsNullOrEmpty(newSymbolName) && !symbols.Contains(newSymbolName))
                {
                    symbols.Add(newSymbolName);
                    symbolStates[newSymbolName] = false;
                    newSymbolName = "";
                    newSymbolField.value = "";
                    SaveCustomSymbols();
                    RefreshUI();
                }
            }).style.width = 50;

            // 4. Action Buttons
            var actionBox = root.Create<VisualElement>();
            actionBox.style.flexDirection = FlexDirection.Row;
            actionBox.style.marginTop = 10;
            
            actionBox.CreateButton("Select All", () =>
            {
                foreach (var s in symbols) symbolStates[s] = true;
                RefreshUI();
            }).SetFlex(1);
            
            actionBox.CreateButton("Deselect All", () =>
            {
                foreach (var s in symbols) symbolStates[s] = false;
                RefreshUI();
            }).SetFlex(1);

            var updateBtn = root.CreateButton("Update/Apply Symbols", () =>
            {
                UpdateScriptingDefineSymbols();
                RefreshSymbolStates();
                RefreshUI();
            });
            updateBtn.style.height = 35;
            updateBtn.style.marginTop = 5;
        }

        private void RefreshUI()
        {
            if (symbolListContainer == null) return;
            symbolListContainer.Clear();

            var sharedSymbols = projectConfig != null ? projectConfig.Symbols : new List<string>();

            foreach (var symbol in symbols)
            {
                var row = symbolListContainer.Create<VisualElement>();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingBottom = 2;

                bool isShared = sharedSymbols.Contains(symbol);
                symbolStates.TryAdd(symbol, false);

                var toggle = row.Create<Toggle>();
                toggle.value = symbolStates[symbol];
                toggle.RegisterValueChangedCallback(evt => symbolStates[symbol] = evt.newValue);

                var symbolLabel = row.CreateLabel(symbol);
                symbolLabel.SetFlex(1);
                symbolLabel.style.marginLeft = 3;
                symbolLabel.OnClick(() => toggle.value = !toggle.value);

                if (isShared)
                {
                    var label = row.CreateLabel("(Shared)");
                    label.style.fontSize = 10;
                    label.style.color = Color.gray;
                    label.style.width = 50;
                }
                else
                {
                    // Add to Config button
                    var addBtn = row.CreateButton("+", () =>
                    {
                        if (projectConfig != null)
                        {
                            if (!projectConfig.Symbols.Contains(symbol))
                            {
                                projectConfig.Symbols.Add(symbol);
                                EditorUtility.SetDirty(projectConfig);
                                AssetDatabase.SaveAssets();
                                SaveCustomSymbols();
                                RefreshUI();
                            }
                        }
                    });
                    addBtn.style.width = 25;
                    addBtn.tooltip = "Add this symbol to Shared Config";

                    row.CreateButton("X", () =>
                    {
                        symbols.Remove(symbol);
                        symbolStates.Remove(symbol);
                        SaveCustomSymbols();
                        RefreshUI();
                    }).style.width = 25;
                }
            }
        }

        private void LoadSymbols()
        {
            symbols = new List<string>();

            if (projectConfig == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:DefineSymbolConfig");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    projectConfig = AssetDatabase.LoadAssetAtPath<DefineSymbolConfig>(path);
                }
            }

            if (projectConfig != null)
            {
                foreach (var s in projectConfig.Symbols)
                {
                    if (!symbols.Contains(s)) symbols.Add(s);
                }
            }
            else
            {
                var defaultSymbols = new List<string>
                {
                    "DEBUG_MODE", "SPINE_ANIMATION", "DOTWEEN", "PRIME_TWEEN",
                    "REMOTE_CONFIG", "ADS", "ANALYTICS", "IAP_ENABLED", "LEVEL_PLAY_ENABLED"
                };
                symbols.AddRange(defaultSymbols);
            }

            string customSymbolsStr = EditorPrefs.GetString(CUSTOM_SYMBOLS_KEY, "");
            if (!string.IsNullOrEmpty(customSymbolsStr))
            {
                var customSymbols = customSymbolsStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in customSymbols)
                {
                    if (!symbols.Contains(s)) symbols.Add(s);
                }
            }

            symbolStates = new Dictionary<string, bool>();
        }

        private void SaveCustomSymbols()
        {
            List<string> sharedSymbols = projectConfig != null ? projectConfig.Symbols : new List<string>();
            var customSymbols = symbols.Where(s => !sharedSymbols.Contains(s)).ToArray();
            string data = string.Join(";", customSymbols);
            EditorPrefs.SetString(CUSTOM_SYMBOLS_KEY, data);
        }

        private void RefreshSymbolStates()
        {
            if (symbols == null) return;
            if (symbolStates == null) symbolStates = new Dictionary<string, bool>();

#if UNITY_6000 || UNITY_2023
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            HashSet<string> currentSymbols = new HashSet<string>(currentDefines.Split(';').Where(s => !string.IsNullOrEmpty(s)));

            foreach (var symbol in symbols)
            {
                symbolStates[symbol] = currentSymbols.Contains(symbol);
            }
        }

        private void CreateConfigAsset()
        {
            var config = ScriptableObject.CreateInstance<DefineSymbolConfig>();
            string path = "Assets/DucMinh/Scripts/Editor/DefineSymbolConfig.asset";
            if (!System.IO.Directory.Exists("Assets/DucMinh/Scripts/Editor"))
            {
                System.IO.Directory.CreateDirectory("Assets/DucMinh/Scripts/Editor");
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            projectConfig = config;
            LoadSymbols();
            RefreshSymbolStates();
            RefreshUI();

            Selection.activeObject = config;
        }

        private void UpdateScriptingDefineSymbols()
        {
            if (symbols == null || symbolStates == null) return;

            try
            {
                BuildTargetGroup[] targets = { BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.iOS };

                foreach (var target in targets)
                {
#if UNITY_6000 || UNITY_2023
                    var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(target);
                    string currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
                    string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
#endif

                    HashSet<string> currentSymbols = new HashSet<string>(currentDefines.Split(';').Where(s => !string.IsNullOrEmpty(s)));

                    foreach (var kvp in symbolStates)
                    {
                        if (kvp.Value) currentSymbols.Add(kvp.Key);
                        else currentSymbols.Remove(kvp.Key);
                    }

                    string newDefines = string.Join(";", currentSymbols);
#if UNITY_6000 || UNITY_2023
                    PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(target, newDefines);
#endif
                }

                Debug.Log("Updated Scripting Define Symbols for all targets.");
                EditorApplication.delayCall += () => AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError($"Update failed: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
