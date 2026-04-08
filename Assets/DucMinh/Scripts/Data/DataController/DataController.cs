using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using DucMinh.UIToolkit;

namespace DucMinh
{
    public static class DataControllerRegistry
    {
        public static readonly List<IDataController> AllControllers = new();

        public static void CreateDemoData()
        {
            if (AllControllers.Count > 0) return;
            DataFactory.CreateIntDataController("demo_int", "Demo Int", 100);
            DataFactory.CreateFloatDataController("demo_float", "Demo Float", 1.5f);
            DataFactory.CreateStringDataController("demo_string", "Demo String", "Hello Antigravity");
            DataFactory.CreateBoolDataController("demo_bool", "Demo Bool", true);
            DataFactory.CreateDateTimeDataController("demo_datetime", "Demo DateTime", DateTime.Now);
            DataFactory.CreateIntArrayDataController("demo_array", "Demo Int Array", new[] { 1, 2, 3, 4, 5 });
        }
    }

    public abstract class BaseDataController<T> : IDataController<T>
    {

        public string Key { get; }
        public string Name { get; }
        protected BaseDataController(string key, string name)
        {
            Key = key;
            Name = name;
            DataControllerRegistry.AllControllers.Add(this);
        }

        public virtual void OnDebugGUI() { }
        public abstract void CreateDebugElement(VisualElement parent);
        public virtual T Value { get; set; }
    }
    
    public class IntDataController : BaseDataController<int>
    {
        private int _tempValue;
        private readonly int _defaultValue;
        public override int Value
        {
            get => StorageService.GetInt(Key, _defaultValue);
            set
            {
                _tempValue = value;
                StorageService.SetInt(Key, value);
            }
        }

        public IntDataController(string key, string name, int defaultValue = 0) : base(key, name)
        {
            _defaultValue = defaultValue;
            _tempValue = Value;
        }

        public override void OnDebugGUI()
        {
            GUIHelper.HorizontalLayout(() =>
            {
                GUIHelper.Label($"{Name}:", true);
                _tempValue = GUIHelper.IntField(_tempValue);
                GUIHelper.Button("Set", () =>
                {
                    if (_tempValue != Value)
                    {
                        Value = _tempValue;
                    }
                }, true);
                GUIHelper.Button("Clear", () =>
                {
                    _tempValue = _defaultValue;
                    StorageService.Delete(Key);
                }, true);
            });
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel($"{Name}:").SetWidthAuto(120).SetFontSizeAuto(14);
            
            var field = row.CreateIntegerField(null, Value, newValue => _tempValue = newValue)
                .SetFlex(1).SetFontSizeAuto(14);
            
            row.CreateButton("Set", () => {
                if (_tempValue != Value) Value = _tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Clear", () => {
                StorageService.Delete(Key);
                _tempValue = _defaultValue;
                field.value = _tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }
    }
    
    public class FloatDataController : BaseDataController<float>
    {
        private string tempText;
        private readonly float defaultValue;
        public override float Value
        {
            get => StorageService.GetFloat(Key, defaultValue);
            set
            {
                tempText = FloatToString(value);
                StorageService.SetFloat(Key, value);
            }
        }

        public FloatDataController(string key, string name, float defaultValue = 0) : base(key, name)
        {
            this.defaultValue = defaultValue;
            tempText = FloatToString(Value);
        }

        public override void OnDebugGUI()
        {
            GUIHelper.HorizontalLayout(() =>
            {
                GUIHelper.Label($"{Name}:", true);
                tempText = GUIHelper.TextField(tempText);
                GUIHelper.Button("Set", () =>
                {
                    string parseText = tempText.Replace(',', '.'); // Hỗ trợ cả dấu phẩy và dấu chấm
                    if (float.TryParse(parseText, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                    {
                        if (Mathf.Abs(result - Value) > 0.001f)
                        {
                            Value = result;
                            tempText = FloatToString(Value);
                        }
                    }
                    else
                    {
                        tempText = FloatToString(Value);
                    }
                }, true);
                GUIHelper.Button("Clear", () =>
                {
                    StorageService.Delete(Key);
                    tempText = FloatToString(defaultValue);
                }, true);
            });
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel($"{Name}:").SetWidthAuto(120).SetFontSizeAuto(14);
            
            var field = row.CreateTextField(null, tempText, newValue => tempText = newValue)
                .SetFlex(1).SetFontSizeAuto(14);
            
            row.CreateButton("Set", () => {
                string parseText = tempText.Replace(',', '.');
                if (float.TryParse(parseText, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    if (Mathf.Abs(result - Value) > 0.001f)
                    {
                        Value = result;
                        tempText = FloatToString(Value);
                        field.value = tempText;
                    }
                }
                else
                {
                    tempText = FloatToString(Value);
                    field.value = tempText;
                }
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Clear", () => {
                StorageService.Delete(Key);
                tempText = FloatToString(defaultValue);
                field.value = tempText;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }

        string FloatToString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
    
    public class StringDataController : BaseDataController<string>
    {
        private string tempValue;
        private readonly string defaultValue;
        public override string Value
        {
            get => StorageService.GetString(Key, defaultValue);
            set
            {
                tempValue = value;
                StorageService.SetString(Key, value);
            }
        }

        public StringDataController(string key, string name, string defaultValue = null) : base(key, name)
        {
            this.defaultValue = defaultValue;
            tempValue = Value;
        }

        public override void OnDebugGUI()
        {
            GUIHelper.HorizontalLayout(() =>
            {
                GUIHelper.Label($"{Name}:", true);
                tempValue = GUIHelper.TextField(tempValue);
                GUIHelper.Button("Set", () =>
                {
                    if (tempValue != Value)
                    {
                        Value = tempValue;
                    }
                }, true);
                GUIHelper.Button("Clear", () =>
                {
                    StorageService.Delete(Key);
                    tempValue = defaultValue;
                }, true);
            });
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel($"{Name}:").SetWidthAuto(120).SetFontSizeAuto(14);
            
            var field = row.CreateTextField(null, tempValue, newValue => tempValue = newValue)
                .SetFlex(1).SetFontSizeAuto(14);
            
            row.CreateButton("Set", () => {
                if (tempValue != Value) Value = tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Clear", () => {
                StorageService.Delete(Key);
                tempValue = defaultValue;
                field.value = tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }
    }
    
    public class BoolDataController : BaseDataController<bool>
    {
        private readonly bool defaultValue;
        public override bool Value
        {
            get => StorageService.GetBool(Key, defaultValue);
            set => StorageService.SetBool(Key, value);
        }

        public BoolDataController(string key, string name, bool defaultValue = false) : base(key, name)
        {
            this.defaultValue = defaultValue;
        }

        public override void OnDebugGUI()
        {
            GUIHelper.HorizontalLayout(() =>
            {
                GUIHelper.Toggle(Name, Value, newValue =>
                {
                    if (newValue != Value) Value = newValue;
                });
            });
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;
            
            row.CreateToggle(Name, Value, newValue => {
                if (newValue != Value) Value = newValue;
            }).SetFontSizeAuto(14);
        }
    }
    
    public class DateTimeController : BaseDataController<DateTime?>
    {
        private DateTime defaultValue;
        private string tempValue;
        public override DateTime? Value
        {
            // get
            // {
            //     if (StorageService.HasKey(Key))
            //     {
            //         return StorageService.GetDateTime(Key, defaultValue);
            //     }
            //     return null;
            // }
            get => StorageService.GetDateTime(Key, defaultValue);
            set
            {
                if (value.HasValue)
                {
                    // tempValue = TimeHelper.ToUnixTimestamp(value.Value).ToString();
                    StorageService.SetDateTime(Key, value.Value);
                }
                else
                {
                    StorageService.Delete(Key);
                }
            }
        }

        public DateTimeController(string key, string name, DateTime defaultValue = default) : base(key, name)
        {
            this.defaultValue = defaultValue;
            tempValue = Value?.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel(Name).SetWidthAuto(120).SetFontSizeAuto(14);
            
            var field = row.CreateTextField(null, tempValue, newValue => tempValue = newValue)
                .SetFlex(1).SetFontSizeAuto(14);
            
            row.CreateButton("Set", () => {
                if (DateTime.TryParse(tempValue, out var dateTime)) {
                    if (Value != dateTime) Value = dateTime;
                }
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Now", () => {
                // Value = TimeHelper.CurrentTime;
                tempValue = Value?.ToString("yyyy-MM-dd HH:mm:ss");
                field.value = tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Clear", () => {
                StorageService.Delete(Key);
                tempValue = null;
                field.value = tempValue;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }
    }

    public class EnumDataController<T> : BaseDataController<T> where T : Enum
    {
        private int tempValue; // Lưu index của enum trong dropdown
        private readonly T defaultValue; // Giá trị mặc định
        private readonly string[] enumNames; // Tên các giá trị enum
        private readonly int maxColumns; // Số cột tối đa trong dropdown

        public EnumDataController(string key, string name, T defaultValue, int maxColumns = 1) : base(key, name)
        {
            this.defaultValue = defaultValue;
            this.maxColumns = maxColumns;
            enumNames = Enum.GetNames(typeof(T));
            tempValue = Array.IndexOf(enumNames, Value.ToString());
        }

        public override T Value
        {
            get
            {
                int intValue = StorageService.GetInt(Key, (int)(object)defaultValue);
                if (Enum.IsDefined(typeof(T), intValue))
                    return (T)(object)intValue;
                return defaultValue;
            }
            set
            {
                tempValue = (int)(object) value;
                StorageService.SetInt(Key, (int)(object) value);
            }
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel($"{Name}:").SetWidthAuto(120).SetFontSizeAuto(14);
            
            var choices = enumNames.ToList();
            var dropdown = row.CreateDropdown(null, choices, Value.ToString(), newValue => {
                T enumValue = (T)Enum.Parse(typeof(T), newValue);
                if (!Equals(enumValue, Value)) {
                    Value = enumValue;
                }
            }).SetFlex(1).SetFontSizeAuto(14);

            row.CreateButton("Clear", () => {
                Value = defaultValue;
                dropdown.value = defaultValue.ToString();
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }
    }

    public class IntArrayDataController : BaseDataController<int[]>
    {
        private string tempText; // Lưu chuỗi nhập liệu từ TextField
        private readonly int[] defaultValue;

        public IntArrayDataController(string key, string name, int[] defaultValue = null) : base(key, name)
        {
            this.defaultValue = defaultValue ?? Array.Empty<int>();
            tempText = ArrayToString(Value); // Khởi tạo từ Value
        }

        public override int[] Value
        {
            get
            {
                string storedString = StorageService.GetString(Key, ArrayToString(defaultValue));
                return StringToArray(storedString);
            }
            set
            {
                var valueText = ArrayToString(value);
                tempText = valueText;
                StorageService.SetString(Key, valueText);
            }
        }

        private string ArrayToString(int[] array)
        {
            return array == null || array.Length == 0 ? "" : string.Join("|", array);
        }

        private int[] StringToArray(string input)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue;
            try
            {
                return input.Split('|')
                    .Select(s => int.Parse(s.Trim()))
                    .ToArray();
            }
            catch
            {
                return null;
            }
        }

        public override void CreateDebugElement(VisualElement parent)
        {
            var row = parent.CreateRow(null).SetMarginBottom(5);
            row.style.alignItems = Align.Center;

            row.CreateLabel($"{Name}:").SetWidthAuto(120).SetFontSizeAuto(14);
            
            var field = row.CreateTextField(null, tempText, newValue => tempText = newValue)
                .SetFlex(1).SetFontSizeAuto(14);
            
            row.CreateButton("Set", () => {
                int[] newArray = StringToArray(tempText);
                if (newArray != null && !newArray.SequenceEqual(Value)) {
                    Value = newArray;
                    tempText = ArrayToString(Value);
                    field.value = tempText;
                }
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);

            row.CreateButton("Clear", () => {
                StorageService.Delete(Key);
                tempText = ArrayToString(defaultValue);
                field.value = tempText;
            }).SetWidthAuto(60).SetHeightAuto(30).SetFontSizeAuto(12);
        }
    }
}