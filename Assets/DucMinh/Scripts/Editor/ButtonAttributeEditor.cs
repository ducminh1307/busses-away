using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DucMinh
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class ButtonAttributeEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, object> _paramInputs = new Dictionary<string, object>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var mono = target as MonoBehaviour;
            if (mono == null) return;

            Type type = mono.GetType();

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                          BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0)
                .ToArray();

            if (methods.Length == 0) return;

            EditorGUILayout.Space();

            foreach (var method in methods)
            {
                var attr = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true).FirstOrDefault();
                string buttonName = !string.IsNullOrEmpty(attr?.Name) ? attr.Name : method.Name;

                var parameters = method.GetParameters();
                object[] paramValues = new object[parameters.Length];
                bool unsupportedType = false;

                if (parameters.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Parameters for {method.Name}:", EditorStyles.boldLabel);

                    foreach (var param in parameters)
                    {
                        string paramKey = GetParamKey(mono, method, param);

                        if (!_paramInputs.ContainsKey(paramKey))
                        {
                            _paramInputs[paramKey] = LoadValue(paramKey, param.ParameterType);
                        }

                        var currentValue = _paramInputs[paramKey];
                        var newValue = DrawParameterField(param, currentValue, out bool supported);

                        if (!supported) unsupportedType = true;

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
                    EditorGUILayout.HelpBox($"One or more parameters in '{method.Name}' are not supported.",
                        MessageType.Warning);
                }

                if (GUILayout.Button(buttonName))
                {
                    InvokeMethod(method, paramValues);
                }

                EditorGUILayout.Space();
            }
        }

        private string GetParamKey(UnityEngine.Object target, MethodInfo method, ParameterInfo param)
        {
            return $"{target.GetInstanceID()}.{method.Name}.{param.Name}";
        }

        private object DrawParameterField(ParameterInfo param, object currentValue, out bool supported)
        {
            var type = param.ParameterType;
            supported = true;

            // Lấy label chuẩn có thể drag để tăng giảm giá trị
            if (type == typeof(string))
            {
                currentValue = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(param.Name), (string)currentValue);
            }
            else if (type == typeof(int))
            {
                currentValue = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(param.Name), (int)currentValue);
            }
            else if (type == typeof(float))
            {
                currentValue = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(param.Name), (float)currentValue);
            }
            else if (type == typeof(bool))
            {
                currentValue = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(param.Name), (bool)currentValue);
            }
            else if (type.IsEnum)
            {
                currentValue = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(param.Name), (Enum)currentValue);
            }
            else
            {
                supported = false;
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(param.Name), $"(Type {type.Name} not supported)");
            }

            return currentValue;
        }


        private object GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
            return null;
        }

        private void SaveValue(string key, object value)
        {
            if (value is string s) EditorPrefs.SetString(key, s);
            else if (value is int i) EditorPrefs.SetInt(key, i);
            else if (value is float f) EditorPrefs.SetFloat(key, f);
            else if (value is bool b) EditorPrefs.SetBool(key, b);
            else if (value is Enum e) EditorPrefs.SetInt(key, Convert.ToInt32(e));
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
                var mb = obj as MonoBehaviour;

                try
                {
                    if (mb != null) Undo.RecordObject(mb, $"Invoke {method.Name}");

                    var invokeTarget = method.IsStatic ? null : obj;
                    method.Invoke(invokeTarget, parameters);

                    if (mb != null) EditorUtility.SetDirty(mb);
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
}