using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public abstract class SpriteAdapter
    {
        public abstract GameObject GameObject { get; }
        public abstract Sprite Sprite { get; set; }
        public abstract Vector2 Size { get; set; }
        public abstract Color Color { get; set; }
        public abstract bool FlipX { get; set; }
        public abstract bool FlipY { get; set; }
        public abstract bool IsVisible { get; set; }
        
        public void SetSprite(Sprite sprite) => Sprite = sprite;

        public static void SetSprite(GameObject gObj, Sprite sprite)
        {
            if (gObj == null)
            {
                Log.Debug($"SetSprite({sprite.name}): GameObject is null");
                return;
            }
            
            var adapter = Create(gObj);
            adapter.SetSprite(sprite);
        }

        public static SpriteAdapter Create(GameObject gObj)
        {
            if (!gObj)
            {
                Log.Debug("SpriteAdapter.Create(): GameObject is null");
                return null;
            }
            
            var image = gObj.GetComponentInChildren<Image>();
            if (image != null)
            {
                return new ImageAdapter(image);
            }
            
            var spriteRenderer = gObj.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return new SpriteRendererAdapter(spriteRenderer);
            }

            return null;
        }

        public static ImageAdapter Create(Image image)
        {
            return new ImageAdapter(image);
        }

        public static SpriteRendererAdapter Create(SpriteRenderer spriteRenderer)
        {
            return new SpriteRendererAdapter(spriteRenderer);
        }
    }


    public class ImageAdapter : SpriteAdapter
    {
        private Image image;

        public ImageAdapter(Image image)
        {
            this.image = image;
        }

        public override GameObject GameObject => image.gameObject;

        public override Sprite Sprite
        {
            get => image.sprite;
            set => image.sprite = value;
        }

        public override Vector2 Size
        {
            get => image.rectTransform.sizeDelta;
            set => image.rectTransform.sizeDelta = value;
        }

        public override Color Color
        {
            get => image.color;
            set => image.color = value;
        }

        public override bool FlipX
        {
            get => image.transform.localScale.x < 0;
            set
            {
                var scale = image.transform.localScale;
                scale.x = value? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                image.transform.localScale = scale;
            }
        }

        public override bool FlipY
        {
            get => image.transform.localScale.y < 0;
            set
            {
                var scale = image.transform.localScale;
                scale.y = value? Mathf.Abs(scale.y) : Mathf.Abs(scale.y);
                image.transform.localScale = scale;
            }
        }

        public override bool IsVisible
        {
            get => image.enabled;
            set => image.enabled = value;
        }
    }

    public class SpriteRendererAdapter : SpriteAdapter
    {
        private SpriteRenderer spriteRenderer;

        public SpriteRendererAdapter(SpriteRenderer spriteRenderer)
        {
            this.spriteRenderer = spriteRenderer;
        }

        public override GameObject GameObject => spriteRenderer.gameObject;

        public override Sprite Sprite
        {
            get => spriteRenderer.sprite;
            set => spriteRenderer.sprite = value;
        }

        public override Vector2 Size
        {
            get => spriteRenderer.bounds.size;
            set
            {
                if (spriteRenderer.drawMode == SpriteDrawMode.Sliced)
                {
                    spriteRenderer.size = value;
                }
                else
                {
                    Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                    var scale = spriteRenderer.transform.localScale;
                    scale.x = value.x / spriteSize.x;
                    scale.y = value.y / spriteSize.y;
                    spriteRenderer.transform.localScale = scale;
                }
            }
        }

        public override Color Color
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = value;
        }

        public override bool FlipX
        {
            get => spriteRenderer.flipX;
            set => spriteRenderer.flipX = value;
        }

        public override bool FlipY
        {
            get => spriteRenderer.flipY;
            set => spriteRenderer.flipY = value;
        }

        public override bool IsVisible
        {
            get => spriteRenderer.enabled;
            set => spriteRenderer.enabled = value;
        }
    }
}