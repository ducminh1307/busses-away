using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public abstract class ColorAdapter
    {
        public abstract GameObject GameObject { get; }
        public abstract Color Color { get; set; }

        public virtual float Alpha
        {
            get => Color.a;
            set
            {
                if (value > 1) value = 1;
                if (value < 0) value = 0;
                var color = Color;
                color.a = value;
                Color = color;
            }
        }
        
        public static void SetColor(GameObject gObj, Color color)
        {
            if (!gObj)
            {
                Log.Warning(gObj.name + " has not been set");
                return;
            }
            
            var colorAdapter = Create(gObj);
            colorAdapter.Color = color;
        }

        #region Create

        public static ColorAdapter Create(GameObject gObj)
        {
            if (gObj == null)
            {
                Log.Warning($"ColorAdapter.Create(): GameObject is null");
                return null;
            }

            var spriteRenderer = gObj.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
                return new SpriteColorAdapter(spriteRenderer);
            
            var image = gObj.GetComponentInChildren<Image>();
            if (image != null)
                return new ImageColorAdapter(image);
            
            var text = gObj.GetComponentInChildren<Text>();
            if (text != null)
                return new TextColorAdapter(text);
            
            var renderer = gObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
                return new RendererColorAdapter(renderer);
            
            var tmpText = gObj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
                return new TMPTextColorAdapter(tmpText);

            Log.Debug($"ColorAdapter.Create(): Color not found in {gObj.name}");
            return null;
        }

        public static ColorAdapter Create(SpriteRenderer spriteRenderer)
        {
            return new SpriteColorAdapter(spriteRenderer);
        }

        public static ColorAdapter Create(Image image)
        {
            return new ImageColorAdapter(image);
        }

        public static ColorAdapter Create(Text text)
        {
            return new TextColorAdapter(text);
        }

        public static ColorAdapter Create(TMP_Text text)
        {
            return new TMPTextColorAdapter(text);
        }

        public static ColorAdapter Create(Renderer renderer)
        {
            return new RendererColorAdapter(renderer);
        }

        #endregion
    }

    public class SpriteColorAdapter : ColorAdapter
    {
        private readonly SpriteRenderer spriteRenderer;

        public SpriteColorAdapter(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
        }

        public override GameObject GameObject => spriteRenderer.gameObject;

        public override Color Color
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = value;
        }
    }

    public class ImageColorAdapter : ColorAdapter
    {
        private readonly Image image;

        public ImageColorAdapter(Image image)
        {
            this.image = image;
        }
        
        public override GameObject GameObject => image.gameObject;

        public override Color Color
        {
            get => image.color;
            set => image.color = value;
        }
    }

    public class RendererColorAdapter : ColorAdapter
    {
        private Renderer renderer;
        private string colorPropertyName = "_Color";
        private int materialIndex = -1; // -1 nghĩa là áp dụng cho tất cả materials

        // Constructor cho tất cả materials
        public RendererColorAdapter(Renderer renderer, string propertyName = "_Color")
        {
            this.renderer = renderer;
            this.colorPropertyName = propertyName;
        }

        // Constructor cho material cụ thể
        public RendererColorAdapter(Renderer renderer, int materialIndex, string propertyName = "_Color")
        {
            this.renderer = renderer;
            this.materialIndex = materialIndex;
            this.colorPropertyName = propertyName;

            // Kiểm tra index hợp lệ
            if (materialIndex >= renderer.materials.Length || materialIndex < 0)
            {
                Log.Warning($"Invalid material index {materialIndex}. Renderer has {renderer.materials.Length} materials.");
                this.materialIndex = -1; // Quay về chế độ tất cả materials
            }
        }

        public override GameObject GameObject => renderer?.gameObject;

        public override Color Color
        {
            get
            {
                if (renderer == null) return Color.white;

                // Nếu chỉ định material cụ thể
                if (materialIndex >= 0 && materialIndex < renderer.materials.Length)
                {
                    Material mat = renderer.materials[materialIndex];
                    return mat.HasProperty(colorPropertyName) ? mat.GetColor(colorPropertyName) : Color.white;
                }

                // Nếu không chỉ định, trả về màu của material đầu tiên (nếu có)
                if (renderer.materials.Length > 0 && renderer.materials[0].HasProperty(colorPropertyName))
                {
                    return renderer.materials[0].GetColor(colorPropertyName);
                }
                return Color.white;
            }
            set
            {
                if (renderer == null) return;

                // Nếu chỉ định material cụ thể
                if (materialIndex >= 0 && materialIndex < renderer.materials.Length)
                {
                    Material mat = renderer.materials[materialIndex];
                    if (mat.HasProperty(colorPropertyName))
                    {
                        mat.SetColor(colorPropertyName, value);
                    }
                }
                // Áp dụng cho tất cả materials
                else
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty(colorPropertyName))
                        {
                            mat.SetColor(colorPropertyName, value);
                        }
                    }
                }
            }
        }

        // Phương thức bổ sung để lấy số lượng materials
        public int MaterialCount => renderer != null ? renderer.materials.Length : 0;

        // Phương thức để thay đổi material index sau khi tạo
        public void SetMaterialIndex(int index)
        {
            if (renderer != null && index >= 0 && index < renderer.materials.Length)
            {
                materialIndex = index;
            }
            else
            {
                materialIndex = -1; // Quay về chế độ tất cả materials
                Log.Warning($"Invalid material index {index}. Renderer has {MaterialCount} materials.");
            }
        }
    }

    public class TextColorAdapter : ColorAdapter
    {
        private readonly Text text;

        public TextColorAdapter(Text text)
        {
            this.text = text;
        }

        public override GameObject GameObject => text?.gameObject;
        public override Color Color
        {
            get => text != null ? text.color : Color.white;
            set { if (text != null) text.color = value; }
        }
    }

    public class TMPTextColorAdapter : ColorAdapter
    {
        private readonly TMP_Text tmpText;

        public TMPTextColorAdapter(TMP_Text tmpText)
        {
            this.tmpText = tmpText;
        }

        public override GameObject GameObject => tmpText?.gameObject;
        public override Color Color
        {
            get => tmpText != null ? tmpText.color : Color.white;
            set { if (tmpText != null) tmpText.color = value; }
        }
    }
}

