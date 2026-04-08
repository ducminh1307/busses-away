using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DucMinh.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DucMinh
{
    // =========================================================
    // PHẦN 1: DROPDOWN DATA DRAWER (Enum / Const)
    // =========================================================
    [CustomPropertyDrawer(typeof(DropdownDataAttribute))]
    public class DropdownDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [DropdownData] with strings.");
                return;
            }

            DropdownDataAttribute attr = attribute as DropdownDataAttribute;
            DropdownDrawerHelper.DrawDropdown(position, property, label, () =>
            {
                // 1. Lấy dữ liệu
                var data = GetDataFromType(attr.SourceType);
                // 2. Lọc trùng
                return DropdownDrawerHelper.FilterDuplicates(property, data, attr.PreventDuplicates);
            });
        }

        private List<string> GetDataFromType(Type type)
        {
            List<string> results = new List<string>();
            if (type.IsEnum)
            {
                results.AddRange(Enum.GetNames(type));
            }
            else
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    // Lấy giá trị của các biến const/static readonly string
                    if (field.FieldType == typeof(string))
                    {
                        var val = field.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(val)) results.Add(val);
                    }
                }
            }
            return results;
        }
    }

    // =========================================================
    // PHẦN 2: DROPDOWN REF DRAWER (Singleton / SO / Scene)
    // =========================================================
    [CustomPropertyDrawer(typeof(DropdownRefAttribute))]
    public class DropdownRefDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [DropdownRef] with strings.");
                return;
            }

            DropdownRefAttribute attr = attribute as DropdownRefAttribute;
            DropdownDrawerHelper.DrawDropdown(position, property, label, () =>
            {
                // 1. Tìm và lấy dữ liệu thông minh
                var data = GetSmartData(attr.SourceType, attr.ListName);
                // 2. Lọc trùng
                return DropdownDrawerHelper.FilterDuplicates(property, data, attr.PreventDuplicates);
            });
        }

        private List<string> GetSmartData(Type type, string fieldName)
        {
            List<string> results = new List<string>();
            object targetInstance = null;

            // A. Check Static List
            FieldInfo staticField = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (staticField != null)
            {
                return ParseList(staticField.GetValue(null) as IEnumerable);
            }

            // B. Check Singleton / Instance
            PropertyInfo instanceProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (instanceProp != null) targetInstance = instanceProp.GetValue(null);

            if (targetInstance == null)
            {
                FieldInfo instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                       ?? type.GetField("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (instanceField != null) targetInstance = instanceField.GetValue(null);
            }

            // C. Check Scene Object (MonoBehaviour)
            if (targetInstance == null && typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                targetInstance = Object.FindFirstObjectByType(type);
            }

            // D. Check ScriptableObject Asset
            if (targetInstance == null && typeof(ScriptableObject).IsAssignableFrom(type))
            {
                string[] guids = AssetDatabase.FindAssets("t:" + type.Name);
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    targetInstance = AssetDatabase.LoadAssetAtPath(path, type);
                }
            }

            if (targetInstance == null) return results;

            // E. Lấy dữ liệu từ Instance tìm được
            FieldInfo listField = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (listField != null)
            {
                return ParseList(listField.GetValue(targetInstance) as IEnumerable);
            }

            return results;
        }

        private List<string> ParseList(IEnumerable collection)
        {
            List<string> list = new List<string>();
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    if (item != null) list.Add(item.ToString());
                }
            }
            return list;
        }
    }

    // =========================================================
    // PHẦN 3: HELPER & UI WINDOW (Core Logic)
    // =========================================================
    public static class DropdownDrawerHelper
    {
        public static void DrawDropdown(Rect position, SerializedProperty property, GUIContent label, Func<List<string>> getDataCallback)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect btnRect = position;

            // Chỉ vẽ Label và chừa chỗ nếu label có nội dung
            if (label != null && !string.IsNullOrEmpty(label.text))
            {
                Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                EditorGUI.LabelField(labelRect, label);

                btnRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);
            }

            // Vẽ Button
            string display = string.IsNullOrEmpty(property.stringValue) ? "Select..." : property.stringValue;

            if (GUI.Button(btnRect, display, EditorStyles.popup))
            {
                // Lazy Load: Chỉ lấy dữ liệu khi bấm nút
                var finalData = getDataCallback.Invoke();
                var dropdown = new GenericStringDropdown(new AdvancedDropdownState(), property, finalData);
                dropdown.Show(btnRect);
            }

            EditorGUI.EndProperty();
        }

        // Logic lọc trùng lặp thông minh (Hỗ trợ cả Wrapper Class)
        public static List<string> FilterDuplicates(SerializedProperty property, List<string> allData, bool prevent)
        {
            if (!prevent) return allData;

            // Kiểm tra xem property có nằm trong Array không
            string path = property.propertyPath;
            if (!path.Contains(".Array.data[")) return allData;

            // Tìm Array cha
            int index = path.LastIndexOf(".Array.data[");
            string arrayPath = path.Substring(0, index);
            SerializedProperty arrayProp = property.serializedObject.FindProperty(arrayPath);

            if (arrayProp == null || !arrayProp.isArray) return allData;

            List<string> usedValues = new List<string>();

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                SerializedProperty element = arrayProp.GetArrayElementAtIndex(i);

                // TH1: List<string> -> Element là string
                if (element.propertyType == SerializedPropertyType.String)
                {
                    string val = element.stringValue;
                    if (!string.IsNullOrEmpty(val)) usedValues.Add(val);
                }
                // TH2: List<Class> -> Element là Generic -> Tìm field con có tên giống field hiện tại
                else if (element.propertyType == SerializedPropertyType.Generic)
                {
                    SerializedProperty child = element.FindPropertyRelative(property.name); // property.name ví dụ là "id"
                    if (child != null && child.propertyType == SerializedPropertyType.String)
                    {
                        string val = child.stringValue;
                        if (!string.IsNullOrEmpty(val)) usedValues.Add(val);
                    }
                }
            }

            // Filter: Giữ lại (Chưa dùng) HOẶC (Đang là chính nó)
            return allData.Where(x => !usedValues.Contains(x) || x == property.stringValue).ToList();
        }
    }

    // Cửa sổ Dropdown Advanced (Có Search)
    public class GenericStringDropdown : AdvancedDropdown
    {
        private SerializedProperty _property;
        private List<string> _data;
        private const string NONE_OPTION = "None"; // Tên hiển thị của nút xóa

        public GenericStringDropdown(AdvancedDropdownState state, SerializedProperty property, List<string> data) :
            base(state)
        {
            _property = property;
            _data = data;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Option");

            // 1. Luôn thêm nút "None" ở vị trí đầu tiên
            var noneItem = new AdvancedDropdownItem(NONE_OPTION);
            // Có thể thêm icon nếu muốn: noneItem.icon = ...
            root.AddChild(noneItem);

            // 2. Thêm các item dữ liệu
            if (_data != null && _data.Count > 0)
            {
                foreach (var item in _data)
                {
                    root.AddChild(new AdvancedDropdownItem(item));
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _property.serializedObject.Update();

            // Logic xử lý khi chọn
            if (item.name == NONE_OPTION)
            {
                // Nếu chọn None -> Xóa dữ liệu (set về rỗng)
                _property.stringValue = string.Empty;
            }
            else
            {
                // Chọn item thường -> Gán giá trị
                _property.stringValue = item.name;
            }

            _property.serializedObject.ApplyModifiedProperties();
        }
    }
}