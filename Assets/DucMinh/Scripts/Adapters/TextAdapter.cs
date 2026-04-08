using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public abstract class TextAdapter
    {
        public abstract GameObject GameObject { get; }
        public abstract string Text { get; set; }
        public abstract Vector3 TextSize { get; }
        public abstract TextAnchor TextAnchor { get; set; }
        public abstract float FontSize { get; set; }
        public abstract bool ResizeEnable { get; set; }
        public abstract Color TextColor { get; set; }
        public abstract bool WrappingEnable { get; set; }

        public void SetText<T>(T text) => Text = text?.ToString() ?? string.Empty;

        public virtual void ForceUpdate()
        {
        }

        #region Static Set Text

        public static void SetText(GameObject gObj, object text)
        {
            SetText(gObj, text.ToString());
        }

        public static void SetText(GameObject gObj, string text)
        {
            if (gObj == null)
            {
                Log.Error($"SetText({text}): GameObject is null");
                return;
            }

            var adapter = Create(gObj);
            adapter.SetText(text);
        }

        #endregion

        #region Create

        public static TextAdapter Create(GameObject gObj)
        {
            if (!gObj)
            {
                Log.Error("TextAdapter.Create(): GameObject is null");
                return null;
            }
            
            TextAdapter adapter = null;
            var textMeshPro = gObj.GetComponentInChildren<TMP_Text>();
            if (textMeshPro != null)
            {
                adapter = new TextMeshProAdapter(textMeshPro);
            }

            var textMesh = gObj.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
            {
                adapter = new TextMeshAdapter(textMesh);
            }

            var uiText = gObj.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                adapter =  new UITextAdapter(uiText);
            }
            
            return adapter;
        }

        public static TextAdapter Create(TMP_Text tmpText)
        {
            return new TextMeshProAdapter(tmpText);
        }

        public static TextAdapter Create(Text uiText)
        {
            return new UITextAdapter(uiText);
        }

        public static TextAdapter Create(TextMesh textMesh)
        {
            return new TextMeshAdapter(textMesh);
        }

        #endregion
    }

    public class TextMeshProAdapter : TextAdapter
    {
        private readonly TMP_Text textMeshPro;

        public TextMeshProAdapter(TMP_Text textMeshPro)
        {
            this.textMeshPro = textMeshPro;
        }

        public override GameObject GameObject => textMeshPro.gameObject;

        public override string Text
        {
            get => textMeshPro.text;
            set => textMeshPro.text = value;
        }

        public override Vector3 TextSize => textMeshPro.textBounds.size;

        public override TextAnchor TextAnchor
        {
            get
            {
                var alignment = textMeshPro.alignment;

                if (alignment == TextAlignmentOptions.TopLeft) return TextAnchor.UpperLeft;
                if (alignment == TextAlignmentOptions.Top) return TextAnchor.UpperCenter;
                if (alignment == TextAlignmentOptions.TopRight) return TextAnchor.UpperRight;

                if (alignment == TextAlignmentOptions.Left) return TextAnchor.MiddleLeft;
                if (alignment == TextAlignmentOptions.Center) return TextAnchor.MiddleCenter;
                if (alignment == TextAlignmentOptions.Right) return TextAnchor.MiddleCenter;

                if (alignment == TextAlignmentOptions.BottomLeft) return TextAnchor.LowerLeft;
                if (alignment == TextAlignmentOptions.Bottom) return TextAnchor.LowerCenter;
                if (alignment == TextAlignmentOptions.BottomRight) return TextAnchor.LowerRight;

                return TextAnchor.MiddleCenter;
            }
            set => textMeshPro.alignment = GetAlignment(value);
        }

        public override float FontSize
        {
            get => textMeshPro.fontSize;
            set => textMeshPro.fontSize = value;
        }

        public override bool ResizeEnable
        {
            get => textMeshPro.enableAutoSizing;
            set => textMeshPro.enableAutoSizing = value;
        }

        public override Color TextColor
        {
            get => textMeshPro.color;
            set => textMeshPro.color = value;
        }

        public override bool WrappingEnable
        {
#if UNITY_6000
            get => textMeshPro.textWrappingMode == TextWrappingModes.Normal;
            set => textMeshPro.textWrappingMode = value? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
#else
            get => textMeshPro.enableWordWrapping;
            set => textMeshPro.enableWordWrapping = value;
#endif
        }

        private TextAlignmentOptions GetAlignment(TextAnchor anchor)
        {
            if (anchor == TextAnchor.UpperLeft) return TextAlignmentOptions.TopLeft;
            if (anchor == TextAnchor.UpperCenter) return TextAlignmentOptions.Top;
            if (anchor == TextAnchor.UpperRight) return TextAlignmentOptions.TopRight;

            if (anchor == TextAnchor.MiddleLeft) return TextAlignmentOptions.Left;
            if (anchor == TextAnchor.MiddleCenter) return TextAlignmentOptions.Center;
            if (anchor == TextAnchor.MiddleRight) return TextAlignmentOptions.Right;

            if (anchor == TextAnchor.LowerLeft) return TextAlignmentOptions.BottomLeft;
            if (anchor == TextAnchor.LowerCenter) return TextAlignmentOptions.Bottom;
            if (anchor == TextAnchor.LowerRight) return TextAlignmentOptions.BottomRight;

            return TextAlignmentOptions.Center;
        }

        public override void ForceUpdate()
        {
            textMeshPro.ForceMeshUpdate();
        }
    }

    public class UITextAdapter : TextAdapter
    {
        private readonly Text uiText;
        private Vector3 cachedTextSize;
        private string lastText;

        public UITextAdapter(Text uiText)
        {
            this.uiText = uiText;
        }

        public override GameObject GameObject => uiText.gameObject;

        public override string Text
        {
            get => uiText.text;
            set => uiText.text = value;
        }

        public override Vector3 TextSize
        {
            get
            {
                if (lastText != uiText.text)
                    UpdateTextSize();

                return cachedTextSize;
            }
        }

        public override TextAnchor TextAnchor
        {
            get => uiText.alignment;
            set => uiText.alignment = value;
        }

        public override float FontSize
        {
            get => uiText.fontSize;
            set => uiText.fontSize = Mathf.RoundToInt(value);
        }

        public override bool ResizeEnable
        {
            get => uiText.resizeTextForBestFit;
            set => uiText.resizeTextForBestFit = value;
        }


        public override Color TextColor
        {
            get => uiText.color;
            set => uiText.color = value;
        }

        public override bool WrappingEnable
        {
            get => uiText.horizontalOverflow == HorizontalWrapMode.Wrap;
            set => uiText.horizontalOverflow = value ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        }
        
        private void UpdateTextSize()
        {
            var generator = new TextGenerator();
            var setting = uiText.GetGenerationSettings(uiText.rectTransform.rect.size);
            var width = generator.GetPreferredWidth(uiText.text, setting);
            var height = generator.GetPreferredHeight(uiText.text, setting);
            cachedTextSize = new Vector3(width, height);
            lastText = uiText.text;
        }

        public override void ForceUpdate()
        {
            UpdateTextSize();
        }
    }

    public class TextMeshAdapter : TextAdapter
    {
        private readonly TextMesh textMesh;

        public TextMeshAdapter(TextMesh textMesh)
        {
            this.textMesh = textMesh;
        }

        public override GameObject GameObject => textMesh.gameObject;

        public override string Text
        {
            get => textMesh.text;
            set => textMesh.text = value;
        }

        public override Vector3 TextSize
        {
            get
            {
                if (textMesh == null || string.IsNullOrEmpty(textMesh.text))
                    return Vector3.zero;
                
                Renderer renderer = textMesh.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Log.Warning($"TextMesh on {GameObject.name} has no Renderer");
                    return Vector3.zero;
                }
                
                return renderer.bounds.size;
            }
        }

        public override TextAnchor TextAnchor
        {
            get => textMesh.anchor;
            set => textMesh.anchor = value;
        }

        public override float FontSize
        {
            get => textMesh.fontSize;
            set => textMesh.fontSize = Mathf.RoundToInt(value);
        }

        public override bool ResizeEnable
        {
            get => false;
            set
            {
                if (value)
                    Log.Warning("TextMesh do not support resize!");
            }
        }

        public override Color TextColor
        {
            get => textMesh.color;
            set => textMesh.color = value;
        }

        public override bool WrappingEnable
        {
            get => false;
            set
            {
                if (value)
                    Log.Warning("TextMesh do not support wrapping!");
            }
        }
    }
}