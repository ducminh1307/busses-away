using System;
using System.Collections.Generic;
using System.Reflection;
using DucMinh.Attributes;
using UnityEditor;
using UnityEngine;

namespace DucMinh.Attribute
{
    [CustomPropertyDrawer(typeof(LanguageCodeSelectorAttribute))]
    public class LanguageCodeSelectorDrawer : PropertyDrawer
    {
        private string[] _displayNames;
        private string[] _values;

        private bool initialized = false;

        private void Init()
        {
            if (initialized) return;

            Type type = typeof(LanguageCode);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            List<string> displayNames = new List<string>();
            List<string> values = new List<string>();

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string))
                {
                    displayNames.Add(field.Name);
                    values.Add((string)field.GetValue(null));
                }
            }

            _displayNames = displayNames.ToArray();
            _values = values.ToArray();

            initialized = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init();

            if (property.propertyType == SerializedPropertyType.String)
            {
                int selectedIndex = Array.IndexOf(_values, property.stringValue);
                if (selectedIndex < 0) selectedIndex = 0;

                EditorGUI.BeginProperty(position, label, property);
                int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, _displayNames);
                property.stringValue = _values[newIndex];
                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [LanguageCode] with string only.");
            }
        }
    }
}
