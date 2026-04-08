using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.UIToolkit
{
    public static partial class UIToolkitHelper
    {
        #region 0. SCALING (Auto Scaling based on Reference Resolution)

        private static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920); // Portrait reference
        private const float MobileDPIScale = 1.5f;

        public static float GetScaleFactor()
        {
            // Use the smaller dimension to scale (standard for mobile)
            float scaleX = Screen.width / ReferenceResolution.x;
            float scaleY = Screen.height / ReferenceResolution.y;
            float scale = Mathf.Min(scaleX, scaleY);

            if (Screen.dpi > 0)
            {
                // Adjust for high DPI screens
                float dpiScale = Screen.dpi / 96f;
                scale *= Mathf.Min(dpiScale, MobileDPIScale);
            }

            return Mathf.Clamp(scale, 0.5f, 4.0f);
        }

        public static float Scale(float value) => value * GetScaleFactor();

        #endregion

        #region 1. HIERARCHY & CREATION (Fluent Interface)

        public static T Create<T>(this VisualElement parent, string name = null, params string[] classes)
            where T : VisualElement, new()
        {
            var element = new T();
            if (!string.IsNullOrEmpty(name)) element.name = name;
            element.AddClasses(classes);

            if (parent != null) parent.Add(element);
            return element;
        }

        public static T Create<T>(this VisualElement parent, string name, Action<T> onSetup, params string[] classes)
            where T : VisualElement, new()
        {
            var element = parent.Create<T>(name, classes);
            onSetup?.Invoke(element);
            return element;
        }

        public static Label CreateLabel(this VisualElement parent, string text, string className = null)
        {
            return parent.Create<Label>(null, l => l.text = text, className);
        }

        public static Button CreateButton(this VisualElement parent, string text, Action onClick, string className = null)
        {
            var btn = parent.Create<Button>(null, button =>
            {
                button.SetText(text);
                button.OnClick(onClick);
            });
            if (!string.IsNullOrEmpty(className)) btn.AddToClassList(className);
            return btn;
        }

        public static TextField CreateTextField(this VisualElement parent, string label, string value = "", Action<string> onValueChanged = null)
        {
            var field = parent.Create<TextField>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static Toggle CreateToggle(this VisualElement parent, string label, bool value = false, Action<bool> onValueChanged = null)
        {
            var toggle = parent.Create<Toggle>(null, t =>
            {
                t.label = label;
                t.value = value;
                if (onValueChanged != null) t.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return toggle;
        }

        public static Foldout CreateFoldout(this VisualElement parent, string title, bool value = true, Action<bool> onValueChanged = null)
        {
            var foldout = parent.Create<Foldout>(null, f =>
            {
                f.text = title;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return foldout;
        }

        public static Slider CreateSlider(this VisualElement parent, string label, float start, float end, float value = 0, Action<float> onValueChanged = null)
        {
            var slider = parent.Create<Slider>(null, s =>
            {
                s.label = label;
                s.lowValue = start;
                s.highValue = end;
                s.value = value;
                if (onValueChanged != null) s.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return slider;
        }

        public static IntegerField CreateIntegerField(this VisualElement parent, string label, int value = 0, Action<int> onValueChanged = null)
        {
            var field = parent.Create<IntegerField>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static FloatField CreateFloatField(this VisualElement parent, string label, float value = 0, Action<float> onValueChanged = null)
        {
            var field = parent.Create<FloatField>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static DropdownField CreateDropdown(this VisualElement parent, string label, List<string> choices, string value = "", Action<string> onValueChanged = null)
        {
            var field = parent.Create<DropdownField>(null, f =>
            {
                f.label = label;
                f.choices = choices;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static HelpBox CreateHelpBox(this VisualElement parent, string message, HelpBoxMessageType type = HelpBoxMessageType.Info)
        {
            var helpBox = parent.Create<HelpBox>(null, h =>
            {
                h.text = message;
                h.messageType = type;
            });
            return helpBox;
        }

        public static MinMaxSlider CreateMinMaxSlider(this VisualElement parent, string label, float minBound, float maxBound, float minValue, float maxValue, Action<Vector2> onValueChanged = null)
        {
            var slider = parent.Create<MinMaxSlider>(null, s =>
            {
                s.label = label;
                s.lowLimit = minBound;
                s.highLimit = maxBound;
                s.minValue = minValue;
                s.maxValue = maxValue;
                if (onValueChanged != null) s.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return slider;
        }

        public static RadioButton CreateRadioButton(this VisualElement parent, string label, bool value = false, Action<bool> onValueChanged = null)
        {
            var rb = parent.Create<RadioButton>(null, r =>
            {
                r.label = label;
                r.value = value;
                if (onValueChanged != null) r.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return rb;
        }

        public static RadioButtonGroup CreateRadioButtonGroup(this VisualElement parent, string label, List<string> choices, int value = 0, Action<int> onValueChanged = null)
        {
            var group = parent.Create<RadioButtonGroup>(null, g =>
            {
                g.label = label;
                g.choices = choices;
                g.value = value;
                if (onValueChanged != null) g.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return group;
        }

        public static Scroller CreateScroller(this VisualElement parent, float lowValue, float highValue, Action<float> onValueChanged, SliderDirection direction = SliderDirection.Vertical)
        {
            var scroller = parent.Create<Scroller>(null, s =>
            {
                s.lowValue = lowValue;
                s.highValue = highValue;
                s.direction = direction;
                if (onValueChanged != null) s.RegisterCallback<ChangeEvent<float>>(evt => onValueChanged(evt.newValue));
            });
            return scroller;
        }

        public static Vector2Field CreateVector2Field(this VisualElement parent, string label, Vector2 value = default, Action<Vector2> onValueChanged = null)
        {
            var field = parent.Create<Vector2Field>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static Vector3Field CreateVector3Field(this VisualElement parent, string label, Vector3 value = default, Action<Vector3> onValueChanged = null)
        {
            var field = parent.Create<Vector3Field>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static Vector2IntField CreateVector2IntField(this VisualElement parent, string label, Vector2Int value = default, Action<Vector2Int> onValueChanged = null)
        {
            var field = parent.Create<Vector2IntField>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static Vector3IntField CreateVector3IntField(this VisualElement parent, string label, Vector3Int value = default, Action<Vector3Int> onValueChanged = null)
        {
            var field = parent.Create<Vector3IntField>(null, f =>
            {
                f.label = label;
                f.value = value;
                if (onValueChanged != null) f.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
            });
            return field;
        }

        public static ProgressBar CreateProgressBar(this VisualElement parent, string title, float value = 0, float lowValue = 0, float highValue = 100)
        {
            var bar = parent.Create<ProgressBar>(null, b =>
            {
                b.title = title;
                b.value = value;
                b.lowValue = lowValue;
                b.highValue = highValue;
            });
            return bar;
        }

        public static T SetName<T>(this T visualElement, string name) where T : VisualElement
        {
            if (string.IsNullOrEmpty(name)) return visualElement;
            visualElement.name = name;
            return visualElement;
        }

        public static T SetText<T>(this T textElement, string text) where T : TextElement, new()
        {
            if (string.IsNullOrEmpty(text)) return textElement;
            textElement.text = text;
            return textElement;
        }

        public static T WithClasses<T>(this T visualElement, string[] classNames) where T : VisualElement
        {
            if (classNames == null) return visualElement;
            foreach (var className in classNames)
            {
                if (!string.IsNullOrEmpty(className)) visualElement.AddToClassList(className);
            }
            return visualElement;
        }

        #endregion

        #region 2. STYLING & LAYOUT

        public static T AddClasses<T>(this T element, params string[] classNames) where T : VisualElement
        {
            if (classNames == null) return element;
            foreach (var cls in classNames)
            {
                if (!string.IsNullOrEmpty(cls)) element.AddToClassList(cls);
            }
            return element;
        }

        public static T SetDisplay<T>(this T element, bool isVisible) where T : VisualElement
        {
            element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            return element;
        }

        public static T SetVisibility<T>(this T element, bool isVisible) where T : VisualElement
        {
            element.style.visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            return element;
        }

        public static T SetSize<T>(this T element, float width, float height) where T : VisualElement
        {
            element.style.width = width;
            element.style.height = height;
            return element;
        }

        public static T SetWidth<T>(this T element, float width) where T : VisualElement
        {
            element.style.width = width;
            return element;
        }

        public static T SetHeight<T>(this T element, float height) where T : VisualElement
        {
            element.style.height = height;
            return element;
        }

        public static T SetMinWidth<T>(this T element, float minWidth) where T : VisualElement
        {
            element.style.minWidth = minWidth;
            return element;
        }

        public static T SetMinHeight<T>(this T element, float minHeight) where T : VisualElement
        {
            element.style.minHeight = minHeight;
            return element;
        }

        public static T SetFlex<T>(this T element, float grow = 0, float shrink = 1) where T : VisualElement
        {
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
            return element;
        }

        public static T SetFlexDirection<T>(this T element, FlexDirection direction) where T : VisualElement
        {
            element.style.flexDirection = direction;
            return element;
        }

        public static T SetFlexWrap<T>(this T element, Wrap wrap) where T : VisualElement
        {
            element.style.flexWrap = wrap;
            return element;
        }

        public static T SetJustifyContent<T>(this T element, Justify justify) where T : VisualElement
        {
            element.style.justifyContent = justify;
            return element;
        }

        public static T SetOverflow<T>(this T element, Overflow overflow) where T : VisualElement
        {
            element.style.overflow = overflow;
            return element;
        }

        public static T SetOpacity<T>(this T element, float opacity) where T : VisualElement
        {
            element.style.opacity = opacity;
            return element;
        }

        public static T SetPadding<T>(this T element, float all) where T : VisualElement
        {
            element.style.paddingLeft = all;
            element.style.paddingRight = all;
            element.style.paddingTop = all;
            element.style.paddingBottom = all;
            return element;
        }

        public static T SetPadding<T>(this T element, float horizontal, float vertical) where T : VisualElement
        {
            element.style.paddingLeft = horizontal;
            element.style.paddingRight = horizontal;
            element.style.paddingTop = vertical;
            element.style.paddingBottom = vertical;
            return element;
        }

        public static T SetPaddingTop<T>(this T element, float value) where T : VisualElement
        {
            element.style.paddingTop = value;
            return element;
        }

        public static T SetPaddingBottom<T>(this T element, float value) where T : VisualElement
        {
            element.style.paddingBottom = value;
            return element;
        }

        public static T SetPaddingLeft<T>(this T element, float value) where T : VisualElement
        {
            element.style.paddingLeft = value;
            return element;
        }

        public static T SetPaddingRight<T>(this T element, float value) where T : VisualElement
        {
            element.style.paddingRight = value;
            return element;
        }

        public static T SetMargin<T>(this T element, float all) where T : VisualElement
        {
            element.style.marginLeft = all;
            element.style.marginRight = all;
            element.style.marginTop = all;
            element.style.marginBottom = all;
            return element;
        }

        public static T SetMargin<T>(this T element, float horizontal, float vertical) where T : VisualElement
        {
            element.style.marginLeft = horizontal;
            element.style.marginRight = horizontal;
            element.style.marginTop = vertical;
            element.style.marginBottom = vertical;
            return element;
        }

        public static T SetMarginTop<T>(this T element, float value) where T : VisualElement
        {
            element.style.marginTop = value;
            return element;
        }

        public static T SetMarginBottom<T>(this T element, float value) where T : VisualElement
        {
            element.style.marginBottom = value;
            return element;
        }

        public static T SetMarginLeft<T>(this T element, float value) where T : VisualElement
        {
            element.style.marginLeft = value;
            return element;
        }

        public static T SetMarginRight<T>(this T element, float value) where T : VisualElement
        {
            element.style.marginRight = value;
            return element;
        }

        #endregion

        #region 3. EVENTS (Mobile Optimized)

        public static T OnClick<T>(this T element, Action action, bool stopPropagation = true) where T : VisualElement
        {
            if (element == null) return null;

            element.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.button != 0) return;

                action?.Invoke();
                if (stopPropagation) evt.StopPropagation();
            });
            return element;
        }

        public static T OnPointerDown<T>(this T element, Action action, bool stopPropagation = true) where T : VisualElement
        {
            if (element == null) return null;

            element.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;

                action?.Invoke();
                if (stopPropagation) evt.StopPropagation();
            });
            return element;
        }

        public static T OnHover<T>(this T element, Action<bool> onHover) where T : VisualElement
        {
            if (element == null) return null;

            element.RegisterCallback<PointerEnterEvent>(evt => onHover?.Invoke(true));
            element.RegisterCallback<PointerLeaveEvent>(evt => onHover?.Invoke(false));
            return element;
        }

        public static T OnValueChanged<T, TV>(this T element, Action<TV> onValueChanged)
            where T : VisualElement, INotifyValueChanged<TV>
        {
            element.RegisterValueChangedCallback(evt => onValueChanged?.Invoke(evt.newValue));
            return element;
        }

        #endregion

        #region 4. LISTVIEW & COLLECTIONS

        public static ListView CreateListView<TData, TElement>(this VisualElement root, IList<TData> itemsSource, System.Func<TElement> makeItem, Action<TElement, int> bindItem, int itemHeight = 40) where TElement : VisualElement
        {
            var listView = new ListView
            {
                itemsSource = (System.Collections.IList)itemsSource,
                makeItem = makeItem,
                bindItem = (e, i) => bindItem((TElement)e, i),

                fixedItemHeight = itemHeight,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                selectionType = SelectionType.None
            };
            root.Add(listView);
            return listView;
        }

        #endregion

        #region 5. FINDING & QUERY

        public static T Find<T>(this VisualElement root, string name = null) where T : VisualElement
        {
            return root.Q<T>(name);
        }

        public static VisualElement Find(this VisualElement root, string name = null)
        {
            return root.Q(name);
        }

        public static List<T> FindAll<T>(this VisualElement root, string name = null) where T : VisualElement
        {
            var list = new List<T>();
            root.Query<T>(name).ForEach(e => list.Add(e));
            return list;
        }

        #endregion

        #region 6. STYLING HELPERS (Colors, Borders, Radius)

        public static T SetBackgroundColor<T>(this T element, Color color) where T : VisualElement
        {
            element.style.backgroundColor = color;
            return element;
        }

        public static T SetBorderColor<T>(this T element, Color color) where T : VisualElement
        {
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            return element;
        }

        public static T SetBorderWidth<T>(this T element, float width) where T : VisualElement
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            return element;
        }

        public static T SetBorderWidth<T>(this T element, float top, float right, float bottom, float left) where T : VisualElement
        {
            element.style.borderTopWidth = top;
            element.style.borderRightWidth = right;
            element.style.borderBottomWidth = bottom;
            element.style.borderLeftWidth = left;
            return element;
        }

        public static T SetBorderRadius<T>(this T element, float radius) where T : VisualElement
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
            return element;
        }

        public static T SetTextColor<T>(this T element, Color color) where T : VisualElement
        {
            element.style.color = color;
            return element;
        }

        public static T SetFontSize<T>(this T element, float size) where T : VisualElement
        {
            element.style.fontSize = size;
            return element;
        }

        public static T SetFontStyleAndWeight<T>(this T element, FontStyle fontStyle) where T : VisualElement
        {
            element.style.unityFontStyleAndWeight = fontStyle;
            return element;
        }

        public static T SetTextAlign<T>(this T element, TextAnchor textAnchor) where T : VisualElement
        {
            element.style.unityTextAlign = textAnchor;
            return element;
        }

        public static T SetFontSizeAuto<T>(this T element, float baseSize) where T : VisualElement
        {
            element.style.fontSize = Scale(baseSize);
            return element;
        }

        public static T SetSizeAuto<T>(this T element, float width, float height) where T : VisualElement
        {
            element.style.width = Scale(width);
            element.style.height = Scale(height);
            return element;
        }

        public static T SetWidthAuto<T>(this T element, float width) where T : VisualElement
        {
            element.style.width = Scale(width);
            return element;
        }

        public static T SetHeightAuto<T>(this T element, float height) where T : VisualElement
        {
            element.style.height = Scale(height);
            return element;
        }

        public static T SetPaddingAuto<T>(this T element, float all) where T : VisualElement
        {
            float s = Scale(all);
            element.style.paddingLeft = s;
            element.style.paddingRight = s;
            element.style.paddingTop = s;
            element.style.paddingBottom = s;
            return element;
        }

        public static T SetMarginAuto<T>(this T element, float all) where T : VisualElement
        {
            float s = Scale(all);
            element.style.marginLeft = s;
            element.style.marginRight = s;
            element.style.marginTop = s;
            element.style.marginBottom = s;
            return element;
        }

        public static T SetBackgroundImage<T>(this T element, Sprite sprite) where T : VisualElement
        {
            element.style.backgroundImage = sprite != null ? new StyleBackground(sprite) : new StyleBackground(StyleKeyword.None);
            return element;
        }

        public static T SetBackgroundImage<T>(this T element, Texture2D texture) where T : VisualElement
        {
            element.style.backgroundImage = texture != null ? new StyleBackground(texture) : new StyleBackground(StyleKeyword.None);
            return element;
        }

        public static T SetBackgroundImage<T>(this T element, StyleBackground background) where T : VisualElement
        {
            element.style.backgroundImage = background;
            return element;
        }

        public static T Scale<T>(this T element, float scale) where T : VisualElement
        {
            element.style.scale = new StyleScale(new Vector2(scale, scale));
            return element;
        }

        #endregion

        #region 7. POSITIONING

        public static T SetPositionAbsolute<T>(this T element, float? top = null, float? left = null, float? bottom = null, float? right = null) where T : VisualElement
        {
            element.style.position = Position.Absolute;
            if (top.HasValue) element.style.top = top.Value;
            if (left.HasValue) element.style.left = left.Value;
            if (bottom.HasValue) element.style.bottom = bottom.Value;
            if (right.HasValue) element.style.right = right.Value;
            return element;
        }

        #endregion

        #region 8. VISIBILITY HELPERS

        public static T Show<T>(this T element) where T : VisualElement
        {
            element.style.display = DisplayStyle.Flex;
            return element;
        }

        public static T Hide<T>(this T element) where T : VisualElement
        {
            element.style.display = DisplayStyle.None;
            return element;
        }

        public static bool IsVisible(this VisualElement element)
        {
            return element.resolvedStyle.display != DisplayStyle.None;
        }

        #endregion

        #region 9. HIERARCHY

        public static void ClearChildren(this VisualElement element)
        {
            element.Clear();
        }

        public static T GetFirstParent<T>(this VisualElement element) where T : VisualElement
        {
            var parent = element.parent;
            while (parent != null)
            {
                if (parent is T typedParent) return typedParent;
                parent = parent.parent;
            }
            return null;
        }

        #endregion

        #region 10. DATA BINDING (Unity 6+)

        public static T SetBindingPath<T>(this T element, string path) where T : VisualElement, IBindable
        {
            element.bindingPath = path;
            return element;
        }
#if UNITY_6_OR_NEWER
        public static T SetDataSource<T>(this T element, object dataSource) where T : VisualElement
        {
            element.dataSource = dataSource;
            return element;
        }
#endif

        #endregion

        #region 11. VECTOR GRAPHICS (Unity 6+)

        public static T SetVectorImage<T>(this T element, VectorImage image) where T : VisualElement
        {
            if (image == null)
            {
                element.style.backgroundImage = null;
                return element;
            }
            element.style.backgroundImage = new StyleBackground(image);
            return element;
        }

        #endregion

        #region 12. MOBILE UTILITIES (Safe Area)

        public static VisualElement PaddingSafeArea(this VisualElement element)
        {
            element.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var panel = element.panel;
                if (panel == null) return;

                var safeArea = Screen.safeArea;
                var panelRect = panel.visualTree.worldBound;

                if (panelRect.width == 0 || panelRect.height == 0) return;

                var scaleX = panelRect.width / Screen.width;
                var scaleY = panelRect.height / Screen.height;

                var left = safeArea.x * scaleX;
                var right = (Screen.width - safeArea.xMax) * scaleX;

                var top = (Screen.height - safeArea.yMax) * scaleY;
                var bottom = safeArea.y * scaleY;

                element.style.paddingLeft = left;
                element.style.paddingRight = right;
                element.style.paddingTop = top;
                element.style.paddingBottom = bottom;
            });
            return element;
        }

        #endregion

        #region 13. DRAG & DROP

        public static UIDragManipulator MakeDraggable(this VisualElement element, Action<VisualElement, Vector2> onDropped = null)
        {
            var manipulator = new UIDragManipulator();
            if (onDropped != null) manipulator.OnDropped += onDropped;
            element.AddManipulator(manipulator);
            return manipulator;
        }

        #endregion

        #region 14. ADVANCED LAYOUT (Tabs, Groups, Dividers)

        public static VisualElement CreateRow(this VisualElement parent, string name = null, params string[] classes)
        {
            var row = parent.Create<VisualElement>(name, classes);
            row.style.flexDirection = FlexDirection.Row;
            return row;
        }

        public static VisualElement CreateColumn(this VisualElement parent, string name = null, params string[] classes)
        {
            var col = parent.Create<VisualElement>(name, classes);
            col.style.flexDirection = FlexDirection.Column;
            return col;
        }

        public static VisualElement CreateDivider(this VisualElement parent, Color? color = null, float height = 1, float margin = 5)
        {
            var divider = parent.Create<VisualElement>("divider");
            divider.style.height = height;
            divider.style.backgroundColor = color ?? new Color(0.3f, 0.3f, 0.3f, 1f);
            divider.style.marginTop = margin;
            divider.style.marginBottom = margin;
            return divider;
        }

        public static ScrollView CreateScrollView(this VisualElement parent, ScrollViewMode mode = ScrollViewMode.Vertical)
        {
            var scroll = parent.Create<ScrollView>();
            scroll.mode = mode;
            return scroll;
        }

        /// <summary>
        /// Creates a simple tab system. 
        /// Returns (HeaderContainer, ContentContainer)
        /// </summary>
        public static (VisualElement header, VisualElement content) CreateTabs(this VisualElement parent, string[] labels, Action<int, VisualElement> onTabSelected)
        {
            var root = parent.Create<VisualElement>("tabs-root");
            var header = root.CreateRow("tab-header");
            var content = root.Create<VisualElement>("tab-content").SetFlex(1);

            var buttons = new List<Button>();

            Action<int> selectTab = (index) =>
            {
                for (int j = 0; j < buttons.Count; j++)
                {
                    if (j == index)
                    {
                        buttons[j].AddToClassList("active");
                        // Optional: inline styling if no classes
                        buttons[j].style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                        buttons[j].style.borderBottomWidth = 2;
                        buttons[j].style.borderBottomColor = Color.white;
                    }
                    else
                    {
                        buttons[j].RemoveFromClassList("active");
                        buttons[j].style.backgroundColor = new StyleColor(StyleKeyword.None);
                        buttons[j].style.borderBottomWidth = 0;
                    }
                }
                content.Clear();
                onTabSelected?.Invoke(index, content);
            };

            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                var btn = header.CreateButton(labels[i], () => selectTab(index))
                    .SetBorderRadius(0)
                    .SetBorderWidth(0);

                btn.style.marginRight = 2;
                buttons.Add(btn);
            }

            if (buttons.Count > 0) selectTab(0);

            return (header, content);
        }

        #endregion
    }
}