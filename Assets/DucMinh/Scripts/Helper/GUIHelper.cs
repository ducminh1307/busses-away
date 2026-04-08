using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DucMinh;

namespace DucMinh
{
    /// <summary>
    /// Helper class for drawing IMGUI elements (Mainly for Runtime Debug UI).
    /// Optimized for mobile with auto-scaling.
    /// </summary>
    public static class GUIHelper
    {
    // Độ phân giải tham chiếu
    private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);
    
    // Kích thước font mặc định
    private static readonly int BaseFontSize = 14; // Tăng để rõ hơn trên di động

    // Hệ số điều chỉnh cho DPI cao
    private static readonly float MobileDPIScale = 2.0f; // Tăng để phù hợp 1080x1920

    // Kích thước mặc định cho slider và dropdown
    private static readonly float BaseSliderWidth = 150f; // Tăng để lớn hơn
    private static readonly float BaseSliderHeight = 150f; // Tăng để lớn hơn
    private static readonly float BaseDropdownHeight = 30f; // Tăng để dropdown rõ hơn

    // Các style tùy chỉnh cho từng loại
    private static GUIStyle _buttonStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _textFieldStyle;
    private static GUIStyle _windowStyle;
    private static GUIStyle _horizontalSliderStyle;
    private static GUIStyle _verticalSliderStyle;
    private static GUIStyle _dropdownStyle;
    private static GUIStyle _dropdownItemStyle;
    private static GUIStyle _tabStyle;
    private static GUIStyle _tabContentStyle;
    private static GUIStyle _lineStyle;
    private static GUIStyle _toggleStyle;

    // Biến lưu trữ để kiểm tra khởi tạo lười
    private static bool _isInitialized;
    private static int _lastScreenWidth;
    private static int _lastScreenHeight;
    private static float _lastDPI;
    
    // Cấu trúc cho một tab
    public struct TabItem
    {
        public string title;
        public Action content;

        public TabItem(string title, Action content)
        {
            this.title = title;
            this.content = content;
        }
    }
    
    // Trạng thái hiển thị dropdown
    private static readonly Dictionary<string, bool> _dropdownStates = new();
    
    // Trạng thái tab đang chọn
    private static readonly Dictionary<string, int> _tabStates = new();

    // Cache để lưu trữ Enum.GetNames
    private static readonly Dictionary<Type, string[]> _enumNamesCache = new();

    // Tính toán tỷ lệ dựa trên độ phân giải màn hình
    private static float GetScaleFactor()
    {
        float scale = Mathf.Max(Screen.width / ReferenceResolution.x, Screen.height / ReferenceResolution.y);
        if (Screen.dpi > 0)
        {
            scale *= Mathf.Min(Screen.dpi / 96f, MobileDPIScale);
        }
        return Mathf.Clamp(scale, 0.75f, 3.0f);
    }

    // Khởi tạo lười các style
    public static void LazyInit()
    {
        if (!_isInitialized || Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight || Screen.dpi != _lastDPI)
        {
            var skin = GUI.skin;
            if (skin == null) return;

            float scale = GetScaleFactor();

            // Style cho Button
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale),
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5),
                normal = { textColor = Color.white },
                hover = { textColor = Color.yellow },
                active = { textColor = Color.gray }
            };

            // Style cho Label
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale * 1.1f),
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                padding = new RectOffset(5, 5, 2, 2)
            };

            // Style cho TextField
            _textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale),
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = GUI.skin.textField.normal.background },
                focused = { textColor = Color.white, background = GUI.skin.textField.focused.background },
                padding = new RectOffset(8, 8, 4, 4)
            };

            // Style cho Window
            _windowStyle = new GUIStyle(GUI.skin.window)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale * 1.2f),
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white },
                padding = new RectOffset(10, 10, 20, 10)
            };

            // Style cho HorizontalSlider
            _horizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedHeight = 20f * scale,
                padding = new RectOffset(5, 5, 5, 5)
            };

            // Style cho VerticalSlider
            _verticalSliderStyle = new GUIStyle(GUI.skin.verticalSlider)
            {
                fixedWidth = 20f * scale,
                padding = new RectOffset(5, 5, 5, 5)
            };

            // Style cho Dropdown (nút chính)
            _dropdownStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 30, 5, 5),
                normal = { textColor = Color.white },
                hover = { textColor = Color.yellow },
                active = { textColor = Color.gray }
            };

            // Style cho các mục trong Dropdown
            _dropdownItemStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale * 0.9f),
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5),
                normal = { textColor = Color.white, background = GUI.skin.box.normal.background },
                hover = { textColor = Color.yellow, background = GUI.skin.box.normal.background },
                active = { textColor = Color.gray }
            };
            
            // Style cho Tab
            _tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale),
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5),
                normal = { textColor = Color.white, background = GUI.skin.button.normal.background },
                hover = { textColor = Color.yellow, background = GUI.skin.button.hover.background },
                active = { textColor = Color.gray, background = GUI.skin.button.active.background }
            };
            
            // Style cho nội dung Tab
            _tabContentStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            _lineStyle = new GUIStyle(GUI.skin.box)
            {
                fixedHeight = 2f * scale, // Độ dày đường kẻ ~4px trên 1080x1920
                normal = { background = Texture2D.grayTexture }
            };
            
            _toggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                fontSize = Mathf.RoundToInt(BaseFontSize * scale), // ~64px trên 1080x1920
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 5, 5),
                normal = { textColor = Color.white },
                hover = { textColor = Color.yellow },
                active = { textColor = Color.gray }
            };

            // Cập nhật trạng thái khởi tạo
            _isInitialized = true;
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastDPI = Screen.dpi;
        }
    }

    // Lấy style với kiểm tra khởi tạo lười
    private static GUIStyle GetStyle(GUIStyle customStyle, GUIStyle defaultStyle)
    {
        LazyInit();
        return customStyle ?? defaultStyle ?? GUI.skin.label; // Fallback nếu defaultStyle null
    }

    private static float GetTextWidth(string text, GUIStyle style)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        return style.CalcSize(new GUIContent(text)).x + 5;
    }

    /// <summary>
    /// Helper to efficiently add a width option to GUILayoutOptions.
    /// </summary>
    private static GUILayoutOption[] AddWidth(GUILayoutOption[] options, float width)
    {
        if (options == null || options.Length == 0) return new[] { GUILayout.Width(width) };
        var list = options.ToList();
        list.Add(GUILayout.Width(width));
        return list.ToArray();
    }

    private static GUILayoutOption[] AddHeight(GUILayoutOption[] options, float height)
    {
        if (options == null || options.Length == 0) return new[] { GUILayout.Height(height) };
        var list = options.ToList();
        list.Add(GUILayout.Height(height));
        return list.ToArray();
    }

    // Tính toán chiều rộng lớn nhất của danh sách văn bản
    private static float GetMaxTextWidth(string[] texts, GUIStyle style)
    {
        float maxWidth = 0f;
        foreach (string text in texts)
        {
            float width = GetTextWidth(text, style);
            if (width > maxWidth) maxWidth = width;
        }
        return maxWidth + 30f; // Thêm padding cho không gian chọn
    }

    // Lấy danh sách tên enum từ cache
    private static string[] GetCachedEnumNames(Type enumType)
    {
        if (!_enumNamesCache.ContainsKey(enumType))
        {
            _enumNamesCache[enumType] = Enum.GetNames(enumType);
        }
        return _enumNamesCache[enumType];
    }

    // Bắt đầu một vùng bố cục ngang
    public static void BeginHorizontal(params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal(options);
    }

    // Kết thúc vùng bố cục ngang
    public static void EndHorizontal()
    {
        GUILayout.EndHorizontal();
    }

    // Bắt đầu một vùng bố cục dọc
    public static void BeginVertical(params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(options);
    }

    // Kết thúc vùng bố cục dọc
    public static void EndVertical()
    {
        GUILayout.EndVertical();
    }

    // Tạo một nút bấm với nhãn, callback và tùy chọn
    public static void Button(string label, Action callback = null, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _buttonStyle);
        if (fitWidthWithText)
        {
            options = AddWidth(options, GetTextWidth(label, style));
        }

        if (GUILayout.Button(label, style, options))
        {
            callback?.Invoke();
        }
    }

    // Tạo một ô nhập văn bản
    public static string TextField(string text, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _textFieldStyle);
        if (fitWidthWithText)
        {
            options = AddWidth(options, GetTextWidth(text, style));
        }
        return GUILayout.TextField(text, style, options);
    }

    // Tạo một nhãn với font động
    public static void Label(string text, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _labelStyle);
        if (fitWidthWithText)
        {
            options = AddWidth(options, GetTextWidth(text, style));
        }
        GUILayout.Label(text, style, options);
    }

    // Tạo khoảng trống
    public static void Space(float pixels)
    {
        GUILayout.Space(pixels * GetScaleFactor());
    }

    // Tạo một vùng bố cục ngang với nội dung được truyền qua delegate
    public static void HorizontalLayout(System.Action content, params GUILayoutOption[] options)
    {
        GUILayout.BeginHorizontal(options);
        content?.Invoke();
        GUILayout.EndHorizontal();
    }

    // Tạo một vùng bố cục dọc với nội dung được truyền qua delegate
    public static void VerticalLayout(System.Action content, params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(options);
        content?.Invoke();
        GUILayout.EndVertical();
    }

    // Tạo một ô nhập số nguyên
    public static int IntField(int value, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _textFieldStyle);
        string text = value.ToString();
        if (fitWidthWithText)
        {
            options = AddWidth(options, GetTextWidth(text, style));
        }
        text = GUILayout.TextField(text, style, options);
        return int.TryParse(text, out int result) ? result : value;
    }

    // Tạo một ô nhập số thực
    public static float FloatField(float value, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _textFieldStyle);
        string text = value.ToString(CultureInfo.InvariantCulture); 
        if (fitWidthWithText)
        {
            options = AddWidth(options, GetTextWidth(text, style));
        }
        text = GUILayout.TextField(text, style, options);

        text = text.Replace(',', '.');
        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        return value; 
    }

    // Tạo một thanh trượt ngang
    public static float HorizontalSlider(float value, float min, float max, bool fitWidthWithText = false, string label = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(null, _horizontalSliderStyle);
        if (fitWidthWithText && !string.IsNullOrEmpty(label))
        {
            float width = GetTextWidth(label, _labelStyle) + (BaseSliderWidth * GetScaleFactor());
            options = options.Append(GUILayout.Width(width)).ToArray();
        }
        else
        {
            options = options.Append(GUILayout.Width(BaseSliderWidth * GetScaleFactor())).ToArray();
        }
        return GUI.HorizontalSlider(GUILayoutUtility.GetRect(GUIContent.none, style, options), value, min, max);
    }

    // Tạo một thanh trượt dọc
    public static float VerticalSlider(float value, float min, float max, bool fitWidthWithText = false, string label = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(null, _verticalSliderStyle);
        if (fitWidthWithText && !string.IsNullOrEmpty(label))
        {
            float width = GetTextWidth(label, _labelStyle) + (BaseSliderHeight * GetScaleFactor());
            options = options.Append(GUILayout.Height(width)).ToArray();
        }
        else
        {
            options = options.Append(GUILayout.Height(BaseSliderHeight * GetScaleFactor())).ToArray();
        }
        return GUI.VerticalSlider(GUILayoutUtility.GetRect(GUIContent.none, style, options), value, min, max);
    }

    // Tạo một dropdown cho enum với bố cục lưới
    public static T EnumDropdown<T>(T selected, string dropdownId, int maxColumns = 4, Action<T> callback = null, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options) where T : Enum
    {
        GUIStyle style = GetStyle(customStyle, _dropdownStyle);
        string[] enumNames = GetCachedEnumNames(typeof(T));
        int selectedIndex = Array.IndexOf(enumNames, selected.ToString());

        // Lấy chiều rộng và cao
        float width = fitWidthWithText ? GetMaxTextWidth(enumNames, style) : BaseSliderWidth * GetScaleFactor();
        float height = BaseDropdownHeight * GetScaleFactor();
        options = options.Append(GUILayout.Width(width)).ToArray();

        // Hiển thị nút chính
        Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(selected.ToString() + " ▼"), style, options.Append(GUILayout.Height(height)).ToArray());
        if (GUI.Button(buttonRect, selected.ToString() + " ▼", style))
        {
            _dropdownStates[dropdownId] = !_dropdownStates.GetValueOrDefault(dropdownId);
        }

        // Xử lý click ngoài để ẩn dropdown
        if (_dropdownStates.GetValueOrDefault(dropdownId) && Event.current.type == EventType.MouseDown && GUIUtility.hotControl == 0)
        {
            float numRows = Mathf.Ceil((float)enumNames.Length / maxColumns);
            Rect dropdownRect = new Rect(buttonRect.x, buttonRect.y + height, width * Mathf.Min(enumNames.Length, maxColumns), height * numRows);
            if (!buttonRect.Contains(Event.current.mousePosition) && !dropdownRect.Contains(Event.current.mousePosition))
            {
                _dropdownStates[dropdownId] = false;
                GUIUtility.hotControl = 0; // Reset để tránh trạng thái không mong muốn
            }
        }

        // Hiển thị danh sách nếu dropdown đang mở
        T newValue = selected;
        if (_dropdownStates.GetValueOrDefault(dropdownId))
        {
            GUILayout.BeginVertical(GUI.skin.box);
            for (int i = 0; i < enumNames.Length; i += maxColumns)
            {
                GUILayout.BeginHorizontal();
                for (int j = i; j < i + maxColumns && j < enumNames.Length; j++)
                {
                    int index = j; // Capture biến để dùng trong lambda
                    if (GUILayout.Button(enumNames[j], _dropdownItemStyle, GUILayout.Height(height), GUILayout.Width(width)))
                    {
                        newValue = (T)Enum.GetValues(typeof(T)).GetValue(index);
                        _dropdownStates[dropdownId] = false; // Ẩn sau khi chọn
                        callback?.Invoke(newValue);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        return newValue;
    }
    
    public static int EnumDropdown(int selectedIndex, string[] enumNames, string dropdownId, int maxColumns = 4, Action<int> callback = null, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _dropdownStyle);
        int validIndex = Mathf.Clamp(selectedIndex, 0, enumNames.Length - 1);
        string selectedText = enumNames[validIndex];

        // Lấy chiều rộng và cao
        float width = fitWidthWithText ? GetMaxTextWidth(enumNames, style) : BaseSliderWidth * GetScaleFactor();
        float height = BaseDropdownHeight * GetScaleFactor();
        options = options.Append(GUILayout.Width(width)).ToArray();

        // Hiển thị nút chính
        Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(selectedText + " ▼"), style, options.Append(GUILayout.Height(height)).ToArray());
        if (GUI.Button(buttonRect, selectedText + " ▼", style))
        {
            _dropdownStates[dropdownId] = !_dropdownStates.GetValueOrDefault(dropdownId);
        }

        // Xử lý click ngoài để ẩn dropdown
        if (_dropdownStates.GetValueOrDefault(dropdownId) && Event.current.type == EventType.MouseDown && !buttonRect.Contains(Event.current.mousePosition))
        {
            float numRows = Mathf.Ceil((float)enumNames.Length / maxColumns);
            Rect dropdownRect = new Rect(buttonRect.x, buttonRect.y + height, width * Mathf.Min(enumNames.Length, maxColumns), height * numRows);
            if (!dropdownRect.Contains(Event.current.mousePosition))
            {
                _dropdownStates[dropdownId] = false;
            }
        }

        // Hiển thị danh sách nếu dropdown đang mở
        int newIndex = validIndex;
        if (_dropdownStates.GetValueOrDefault(dropdownId))
        {
            GUILayout.BeginVertical(GUI.skin.box);
            for (int i = 0; i < enumNames.Length; i += maxColumns)
            {
                GUILayout.BeginHorizontal();
                for (int j = i; j < i + maxColumns && j < enumNames.Length; j++)
                {
                    int index = j;
                    if (GUILayout.Button(enumNames[j], _dropdownItemStyle, GUILayout.Height(height), GUILayout.Width(width)))
                    {
                        newIndex = index;
                        _dropdownStates[dropdownId] = false;
                        callback?.Invoke(newIndex);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        return newIndex;
    }
    
    // Tạo một hệ thống tab
    public static int TabControl(string tabId, TabItem[] tabs, ref Vector2 scrollPosition, float maxWidth = 0f, GUIStyle customTabStyle = null, GUIStyle customContentStyle = null, params GUILayoutOption[] options)
    {
        if (tabs == null || tabs.Length == 0 || tabs.Any(t => t.title == null || t.content == null))
        {
            return -1; // Trả về -1 nếu đầu vào không hợp lệ
        }

        // Lấy hoặc khởi tạo trạng thái tab
        if (!_tabStates.ContainsKey(tabId))
        {
            _tabStates[tabId] = 0; // Mặc định chọn tab đầu tiên
        }
        int selectedTab = _tabStates[tabId];

        // Lấy style và kích thước
        GUIStyle tabStyle = GetStyle(customTabStyle, _tabStyle);
        GUIStyle contentStyle = GetStyle(customContentStyle, _tabContentStyle);
        float height = BaseDropdownHeight * GetScaleFactor();
        float maxLayoutWidth = maxWidth > 0f ? maxWidth : Screen.width * 0.9f; // Giới hạn 90% màn hình nếu không chỉ định
        // Tính toán chiều rộng các nút
        float[] buttonWidths = tabs.Select(tab => GetTextWidth(tab.title, tabStyle) + 20f).ToArray(); // Padding 20px
        float currentRowWidth = 0f;
        int rowCount = 1;

        // Hiển thị các nút tab theo lưới
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            float buttonWidth = buttonWidths[i] + (5f * GetScaleFactor()); // Thêm khoảng cách giữa nút
            if (currentRowWidth + buttonWidth > maxLayoutWidth && currentRowWidth > 0f)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                currentRowWidth = 0f;
                rowCount++;
            }

            GUIStyle currentStyle = new GUIStyle(tabStyle)
            {
                normal =
                {
                    background = (i == selectedTab) ? tabStyle.active.background : tabStyle.normal.background ,
                    textColor = (i == selectedTab)? Color.yellow : Color.white,
                },
                fontStyle = (i == selectedTab) ? FontStyle.Bold : FontStyle.Normal
            };
            if (GUILayout.Button(tabs[i].title, currentStyle, GUILayout.Height(height), GUILayout.Width(buttonWidths[i])))
            {
                _tabStates[tabId] = index;
                selectedTab = index;
                scrollPosition = Vector2.zero;
            }
            GUILayout.Space(5f * GetScaleFactor()); // Khoảng cách giữa các nút
            currentRowWidth += buttonWidth;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        Space(10);
        HorizontalLine();
        Space(10);

        // Hiển thị nội dung của tab được chọn
        
        VerticalScrollView(ref scrollPosition, () =>
        {
            if (selectedTab >= 0 && selectedTab < tabs.Length)
            {
                tabs[selectedTab].content?.Invoke();
            }
        });

        return selectedTab;
    }
    
    public static void VerticalScrollView(ref Vector2 scrollPosition, Action content, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _tabContentStyle);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, style, options);
        GUILayout.BeginVertical();
        content?.Invoke();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    // Đường kẻ ngang
    public static void HorizontalLine(GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _lineStyle);
        GUILayout.Box("", style, options.Append(GUILayout.Height(2f * GetScaleFactor())).Append(GUILayout.ExpandWidth(true)).ToArray());
    }
    
    public static bool Toggle(string label, bool value, Action<bool> callback = null, bool fitWidthWithText = false, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        GUIStyle style = GetStyle(customStyle, _toggleStyle);
        if (fitWidthWithText)
        {
            float width = GetTextWidth(label, style) + (20f * GetScaleFactor()); // Thêm khoảng cách cho hộp kiểm
            options = options.Append(GUILayout.Width(width)).ToArray();
        }
        bool newValue = GUILayout.Toggle(value, label, style, options);
        if (newValue != value)
        {
            callback?.Invoke(newValue);
        }
        return newValue;
    }

    // Tạo một cửa sổ GUI với nội dung được truyền qua delegate và các tùy chọn style
    public static Rect Window(int id, Rect windowRect, Action<int> content, string title, Action<bool> onClose = null, GUIStyle customStyle = null, params GUILayoutOption[] options)
    {
        LazyInit();
        return GUI.Window(id, windowRect, (windowID) =>
        {
            VerticalLayout(() =>
            {
                WindowTitle(title, true, onClose);
                Space(20);
                content?.Invoke(windowID);
            }, options);
        }, "", GetStyle(customStyle, _windowStyle));
    }

    // Tạo một cửa sổ có thể kéo thả với thanh tiêu đề
    public static Rect DraggableWindow(int id, Rect windowRect, System.Action<int> content, string title, Action<bool> onClose = null, GUIStyle customStyle = null)
    {
        LazyInit();
        return GUI.Window(id, windowRect, (windowID) =>
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 20 * GetScaleFactor()));
            VerticalLayout(() =>
            {
                WindowTitle(title, true, onClose);
                content?.Invoke(windowID);
            });
        }, "", GetStyle(customStyle, _windowStyle));
    }

    // Tạo một tiêu đề cửa sổ với nút đóng
    public static void WindowTitle(string title, bool showCloseButton = true, Action<bool> onClose = null)
    {
        bool close = false;
        HorizontalLayout(() =>
        {
            Label(title, true, _labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            if (showCloseButton)
            {
                Button("X", () => close = true, true, _buttonStyle, GUILayout.Width(20 * GetScaleFactor()));
            }
        });
        onClose?.Invoke(close);
    }
    // Tiễn biệt class
}
}