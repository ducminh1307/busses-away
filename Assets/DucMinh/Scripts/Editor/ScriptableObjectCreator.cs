using System;
using System.Collections.Generic;
using System.Linq;
using DucMinh.UIToolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace DucMinh
{
    public static class ScriptableObjectCreator
    {
        [MenuItem("Assets/Create/Scriptable Object Creator")]
        public static void CreateScriptableObject()
        {
            var scriptableObjectTypes = GetScriptableObjectTypes();
            ScriptableObjectWindow.ShowWindow(scriptableObjectTypes);
        }

        private static List<Type> GetScriptableObjectTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ScriptableObject).IsAssignableFrom(type)
                               && !type.IsAbstract
                               && !type.IsInterface
                               && type.IsPublic
                               && !type.IsGenericTypeDefinition
                               && !IsEditorType(type)
                               && !IsUnityInternal(type))
                .ToList();
        }

        private static bool IsEditorType(Type type)
        {
            return type.IsSubclassOf(typeof(UnityEditor.Editor)) ||
                   type.IsSubclassOf(typeof(EditorWindow)) ||
                   type.IsSubclassOf(typeof(PropertyDrawer)) ||
                   type.IsSubclassOf(typeof(ScriptableWizard));
        }

        private static bool IsUnityInternal(Type type)
        {
            var ns = type.Namespace ?? "";
            var asmName = type.Assembly.GetName().Name;

            if (ns.StartsWith("UnityEngine") ||
                ns.StartsWith("UnityEditor") ||
                ns.StartsWith("Unity.") ||
                ns.StartsWith("System") ||
                ns.StartsWith("TMPro") ||
                ns.StartsWith("Microsoft"))
            {
                return true;
            }

            if (asmName.StartsWith("UnityEngine") ||
                asmName.StartsWith("UnityEditor") ||
                asmName.StartsWith("Unity.") ||
                asmName.StartsWith("com.unity") ||
                asmName.StartsWith("System") ||
                asmName.StartsWith("mscorlib"))
            {
                return true;
            }

            return false;
        }
    }

    public class ScriptableObjectWindow : EditorWindow
    {
        private static List<Type> types;
        private string searchString = "";
        private List<Type> filteredTypes;

        private VisualElement listContainer;
        private Label countLabel;

        public static void ShowWindow(List<Type> scriptableTypes)
        {
            types = scriptableTypes;
            var window = GetWindow<ScriptableObjectWindow>();
            window.titleContent = new GUIContent("Create Scriptable Object");
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        public void CreateGUI()
        {
            if (types == null || types.Count == 0)
            {
                rootVisualElement.CreateLabel("No ScriptableObject types found!");
                return;
            }

            rootVisualElement.style.paddingLeft = 10;
            rootVisualElement.style.paddingRight = 10;
            rootVisualElement.style.paddingTop = 10;
            rootVisualElement.style.paddingBottom = 10;

            // Search Header
            var searchField = new ToolbarSearchField();
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchString = evt.newValue;
                RefreshUI();
            });
            rootVisualElement.Add(searchField);

            countLabel = rootVisualElement.CreateLabel("");
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            countLabel.style.marginTop = 5;

            // Scrollable List
            var scrollView = rootVisualElement.Create<ScrollView>();
            scrollView.style.flexGrow = 1;
            scrollView.style.marginTop = 5;
            scrollView.SetBorderWidth(1);
            scrollView.SetBorderColor(Color.gray);
            
            listContainer = scrollView.contentContainer;

            RefreshUI();
        }

        private void RefreshUI()
        {
            UpdateFilteredTypes();
            
            if (countLabel != null)
                countLabel.text = $"Found {filteredTypes.Count} ScriptableObjects";

            if (listContainer == null) return;
            listContainer.Clear();

            foreach (var type in filteredTypes)
            {
                var btn = listContainer.CreateButton(type.Name, () =>
                {
                    CreateAsset(type);
                    Close();
                });
                btn.style.unityTextAlign = TextAnchor.MiddleLeft;
                btn.style.marginTop = 2;
                btn.style.marginBottom = 2;
                btn.tooltip = type.FullName;
            }
        }

        private void UpdateFilteredTypes()
        {
            if (string.IsNullOrEmpty(searchString))
            {
                filteredTypes = new List<Type>(types);
            }
            else
            {
                filteredTypes = types
                    .Where(t => t.Name.ToLower().Contains(searchString.ToLower()))
                    .ToList();
            }
        }

        private void CreateAsset(Type type)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(type);

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(path)))
            {
                path = System.IO.Path.GetDirectoryName(path);
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{type.Name}.asset");

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}