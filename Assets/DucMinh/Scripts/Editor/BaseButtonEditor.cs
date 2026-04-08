using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object; // Alias để tránh nhầm lẫn

namespace DucMinh
{
    // 1. Tách logic chính ra một class abstract không gắn CustomEditor trực tiếp
    public abstract class BaseButtonEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, object> _paramInputs = new Dictionary<string, object>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Sửa đổi: Không ép kiểu về MonoBehaviour nữa, dùng Object chung
            var targetObj = target; 
            if (targetObj == null) return;

            Type type = targetObj.GetType();

            // Tìm các method có attribute [Button]
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel); // Thêm tiêu đề cho gọn

            foreach (var method in methods)
            {
                DrawMethodButton(method, targetObj);
            }
        }

        private void DrawMethodButton(MethodInfo method, Object targetObj)
        {
            var attr = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true).FirstOrDefault();
            string buttonName = !string.IsNullOrEmpty(attr?.Name) ? attr.Name : ObjectNames.NicifyVariableName(method.Name);

            var parameters = method.GetParameters();
            object[] paramValues = new object[parameters.Length];
            bool unsupportedType = false;

            // Vẽ khung tham số nếu có
            if (parameters.Length > 0)
            {
                EditorGUILayout.BeginVertical("helpbox"); // Dùng helpbox nhìn đẹp hơn box thường
                EditorGUILayout.LabelField($"{buttonName} Params:", EditorStyles.miniBoldLabel);

                foreach (var param in parameters)
                {
                    string paramKey = GetParamKey(targetObj, method, param);

                    // Load giá trị cũ nếu chưa có
                    if (!_paramInputs.ContainsKey(paramKey))
                    {
                        _paramInputs[paramKey] = LoadValue(paramKey, param.ParameterType);
                    }

                    var currentValue = _paramInputs[paramKey];
                    var newValue = DrawParameterField(param, currentValue, out bool supported);

                    if (!supported) unsupportedType = true;

                    // Lưu giá trị mới nếu thay đổi
                    if (!Equals(currentValue, newValue))
                    {
                        _paramInputs[paramKey] = newValue;
                        SaveValue(paramKey, newValue);
                    }

                    paramValues[param.Position] = newValue;
                }
                EditorGUILayout.EndVertical();
            }

            if (unsupportedType)
            {
                EditorGUILayout.HelpBox($"Cannot verify parameters for '{method.Name}'.", MessageType.Warning);
            }

            // Vẽ nút bấm
            // GUI.backgroundColor = Color.cyan; // Có thể thêm màu nếu thích
            if (GUILayout.Button(buttonName, GUILayout.Height(30)))
            {
                InvokeMethod(method, paramValues);
            }
            // GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(5);
        }

        private string GetParamKey(Object target, MethodInfo method, ParameterInfo param)
        {
            // Dùng GetInstanceID là chuẩn cho cả SO và MB
            return $"{target.GetInstanceID()}.{method.Name}.{param.Name}";
        }

        private object DrawParameterField(ParameterInfo param, object currentValue, out bool supported)
        {
            var type = param.ParameterType;
            supported = true;
            string label = ObjectNames.NicifyVariableName(param.Name);

            if (type == typeof(string)) return EditorGUILayout.TextField(label, (string)currentValue);
            if (type == typeof(int)) return EditorGUILayout.IntField(label, (int)currentValue);
            if (type == typeof(float)) return EditorGUILayout.FloatField(label, (float)currentValue);
            if (type == typeof(bool)) return EditorGUILayout.Toggle(label, (bool)currentValue);
            if (type.IsEnum) return EditorGUILayout.EnumPopup(label, (Enum)currentValue);
            if (type == typeof(Vector2)) return EditorGUILayout.Vector2Field(label, (Vector2)currentValue); // Thêm Vector2
            if (type == typeof(Vector3)) return EditorGUILayout.Vector3Field(label, (Vector3)currentValue); // Thêm Vector3
            
            supported = false;
            EditorGUILayout.LabelField(label, $"(Type {type.Name} not supported)");
            return currentValue;
        }

        // ... Các hàm LoadValue/SaveValue/GetDefaultValue giữ nguyên như cũ ...
        // (Tôi đã lược bớt để tập trung vào phần thay đổi logic, bạn copy lại phần thân hàm cũ vào đây nhé)
        // Lưu ý: Cần thêm hỗ trợ Vector2/Vector3 trong Save/Load nếu bạn muốn hỗ trợ thêm type đó.

        private object GetDefaultValue(Type type)
        {
             if (type == typeof(string)) return "";
             if (type == typeof(int)) return 0;
             if (type == typeof(float)) return 0f;
             if (type == typeof(bool)) return false;
             if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
             if (type == typeof(Vector2)) return Vector2.zero;
             if (type == typeof(Vector3)) return Vector3.zero;
             return null;
        }

        private void SaveValue(string key, object value)
        {
            if (value is string s) EditorPrefs.SetString(key, s);
            else if (value is int i) EditorPrefs.SetInt(key, i);
            else if (value is float f) EditorPrefs.SetFloat(key, f);
            else if (value is bool b) EditorPrefs.SetBool(key, b);
            else if (value is Enum e) EditorPrefs.SetInt(key, Convert.ToInt32(e));
            // Vector cần lưu dạng Json hoặc từng thành phần, đơn giản nhất là bỏ qua lưu Vector tạm thời
        }

        private object LoadValue(string key, Type type)
        {
            if (type == typeof(string)) return EditorPrefs.GetString(key, "");
            if (type == typeof(int)) return EditorPrefs.GetInt(key, 0);
            if (type == typeof(float)) return EditorPrefs.GetFloat(key, 0f);
            if (type == typeof(bool)) return EditorPrefs.GetBool(key, false);
            if (type.IsEnum) return Enum.ToObject(type, EditorPrefs.GetInt(key, 0));
            return GetDefaultValue(type);
        }

        private void InvokeMethod(MethodInfo method, object[] parameters)
        {
            foreach (var obj in targets)
            {
                // Sửa đổi: Không cast as MonoBehaviour nữa
                var targetObj = obj;

                try
                {
                    if (targetObj != null) Undo.RecordObject(targetObj, $"Invoke {method.Name}");

                    var invokeTarget = method.IsStatic ? null : targetObj;
                    method.Invoke(invokeTarget, parameters);

                    // Sửa đổi: SetDirty cho Object chung
                    if (targetObj != null) EditorUtility.SetDirty(targetObj);
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogException(tie.InnerException ?? tie);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    // 2. Class Adapter cho MonoBehaviour
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourButtonEditor : BaseButtonEditor
    {
    }

    // 3. Class Adapter cho ScriptableObject
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ScriptableObjectButtonEditor : BaseButtonEditor
    {
    }
}