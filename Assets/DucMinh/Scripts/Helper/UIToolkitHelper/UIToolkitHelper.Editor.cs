#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.UIToolkit
{
    public static partial class UIToolkitHelper
    {
        public static MaskField CreateMaskField(this VisualElement root, string label, string name = "new-mask-field", bool enable = true, List<string> choices = null, int defaultMask = 0, params string[] classNames)
        {
            var customChooses = choices ?? new List<string>();
            var maskField = new MaskField(label, customChooses, defaultMask);
            maskField.SetName(name).WithClasses(classNames);
            maskField.SetEnabled(enable);
            root.Add(maskField);
            return maskField;
        }

        public static PropertyField CreatePropertyField(this VisualElement root, UnityEditor.SerializedProperty property, string label = null, string name = null)
        {
            var field = new PropertyField(property, label);
            if (!string.IsNullOrEmpty(name)) field.name = name;
            root.Add(field);
            return field;
        }

        public static ColorField CreateColorField(this VisualElement root, string label, Color value, bool showAlpha = true, Action<Color> onValueChanged = null)
        {
            var field = new ColorField(label) { value = value, showAlpha = showAlpha };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static CurveField CreateCurveField(this VisualElement root, string label, AnimationCurve value, Action<AnimationCurve> onValueChanged = null)
        {
            var field = new CurveField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static GradientField CreateGradientField(this VisualElement root, string label, Gradient value, Action<Gradient> onValueChanged = null)
        {
            var field = new GradientField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static EnumField CreateEnumField(this VisualElement root, string label, Enum defaultValue, Action<Enum> onValueChanged = null)
        {
            var field = new EnumField(label, defaultValue);
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static ObjectField CreateObjectField(this VisualElement root, string label, Type objectType, bool allowSceneObjects, UnityEngine.Object value = null, Action<UnityEngine.Object> onValueChanged = null)
        {
            var field = new ObjectField(label)
            {
                objectType = objectType,
                allowSceneObjects = allowSceneObjects,
                value = value
            };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static RectField CreateRectField(this VisualElement root, string label, Rect value, Action<Rect> onValueChanged = null)
        {
            var field = new RectField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static RectIntField CreateRectIntField(this VisualElement root, string label, RectInt value, Action<RectInt> onValueChanged = null)
        {
            var field = new RectIntField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static BoundsField CreateBoundsField(this VisualElement root, string label, Bounds value, Action<Bounds> onValueChanged = null)
        {
            var field = new BoundsField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static LayerField CreateLayerField(this VisualElement root, string label, int value, Action<int> onValueChanged = null)
        {
            var field = new LayerField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        public static TagField CreateTagField(this VisualElement root, string label, string value, Action<string> onValueChanged = null)
        {
            var field = new TagField(label) { value = value };
            if (onValueChanged != null) field.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            root.Add(field);
            return field;
        }

        #region EDITOR TOOLS (Toolbar, Tabs)

        public static Toolbar CreateToolbar(this VisualElement root)
        {
            var toolbar = new Toolbar();
            root.Add(toolbar);
            return toolbar;
        }

        public static ToolbarButton CreateToolbarButton(this Toolbar toolbar, string text, Action onClick)
        {
            var btn = new ToolbarButton(onClick) { text = text };
            toolbar.Add(btn);
            return btn;
        }

        public static ToolbarToggle CreateToolbarToggle(this Toolbar toolbar, string text, bool value, Action<bool> onValueChanged)
        {
            var toggle = new ToolbarToggle { text = text, value = value };
            toggle.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));
            toolbar.Add(toggle);
            return toggle;
        }

        public static ToolbarMenu CreateToolbarMenu(this Toolbar toolbar, string text)
        {
            var menu = new ToolbarMenu { text = text };
            toolbar.Add(menu);
            return menu;
        }

        public static (Toolbar header, VisualElement content) CreateEditorTabs(this VisualElement root, string[] labels, int initialIndex, Action<int, VisualElement> onTabSelected)
        {
            var toolbar = root.CreateToolbar();
            var content = root.Create<VisualElement>("editor-tab-content").SetFlex(1);
            
            var toggles = new List<ToolbarToggle>();

            Action<int> selectTab = (index) =>
            {
                for (int j = 0; j < toggles.Count; j++)
                {
                    toggles[j].SetValueWithoutNotify(j == index);
                }
                content.Clear();
                onTabSelected?.Invoke(index, content);
            };

            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                var toggle = toolbar.CreateToolbarToggle(labels[i], false, (val) => {
                    if (val) selectTab(index);
                    else toggles[index].SetValueWithoutNotify(true);
                });
                toggles.Add(toggle);
            }

            if (toggles.Count > 0) selectTab(Mathf.Clamp(initialIndex, 0, toggles.Count - 1));

            return (toolbar, content);
        }

        #endregion
    }
}
#endif