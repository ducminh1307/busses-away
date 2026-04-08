using System;
using System.Text.RegularExpressions;
using DucMinh.Attributes;
using UnityEditor;
using UnityEngine;

namespace DucMinh
{
    [CustomPropertyDrawer(typeof(ElementNameAttribute))]
    public class ElementNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var elementName = attribute as ElementNameAttribute;
            if (elementName != null)
            {
                // Kiểm tra nếu là phần tử của array hoặc list
                if (property.propertyPath.Contains(".Array.data["))
                {
                    // Lấy index từ propertyPath
                    int index = GetArrayElementIndex(property);
                    if (index >= 0)
                    {
                        // Nếu có enumType, sử dụng tên giá trị enum
                        if (elementName.EnumType != null)
                        {
                            string[] enumNames = Enum.GetNames(elementName.EnumType);
                            if (index < enumNames.Length)
                            {
                                label.text = enumNames[index];
                            }
                            else
                            {
                                label.text = $"";
                            }
                        }
                        else
                        {
                            // Nếu không có enumType, sử dụng tên + index
                            label.text = $"{elementName.Name} {index}";
                        }
                    }
                    else
                    {
                        label.text = elementName.Name;
                    }
                }
                else
                {
                    // Hiển thị tên cho field đơn
                    label.text = elementName.Name;
                }
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        private int GetArrayElementIndex(SerializedProperty property)
        {
            string path = property.propertyPath;
            // Tìm tất cả các đoạn .Array.data[index]
            var matches = Regex.Matches(path, @"\.Array\.data\[(\d+)\]");
            if (matches.Count > 0)
            {
                // Lấy index của đoạn cuối cùng (phần tử hiện tại)
                return int.Parse(matches[matches.Count - 1].Groups[1].Value);
            }
            return -1; // Trả về -1 nếu không phải phần tử array/list
        }
    }
}