using UnityEngine;

namespace DucMinh
{
    public enum IconScaleMode
    {
        Fit,
        Fill,
        Stretch
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class IconResizer : MonoBehaviour, ISetSize
    {
        [Header("Settings")]
        [SerializeField] private IconScaleMode scaleMode = IconScaleMode.Fit;

        [SerializeField] private Vector2 pivotOffset = Vector2.zero;

        private SpriteRenderer _spriteRenderer;

        public void SetSize(float width, float height)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

            _spriteRenderer.drawMode = SpriteDrawMode.Simple;

            transform.localScale = Vector3.one;
            
            Vector2 originalSize = _spriteRenderer.sprite.bounds.size;
            
            if (originalSize.x == 0 || originalSize.y == 0) return;

            var scaleX = width / originalSize.x;
            var scaleY = height / originalSize.y;
            
            var finalScaleX = 1f;
            var finalScaleY = 1f;

            switch (scaleMode)
            {
                case IconScaleMode.Fit:
                    var minScale = Mathf.Min(scaleX, scaleY);
                    finalScaleX = minScale;
                    finalScaleY = minScale;
                    break;

                case IconScaleMode.Fill:
                    var maxScale = Mathf.Max(scaleX, scaleY);
                    finalScaleX = maxScale;
                    finalScaleY = maxScale;
                    break;

                case IconScaleMode.Stretch:
                    finalScaleX = scaleX;
                    finalScaleY = scaleY;
                    break;
            }

            transform.localScale = new Vector3(finalScaleX, finalScaleY, 1f);

            if (pivotOffset != Vector2.zero)
            {
                transform.localPosition = new Vector3(pivotOffset.x, pivotOffset.y, transform.localPosition.z);
            }
        }

#if DEBUG_MODE
        [Button("Test Resize")]
        public void TestResize(float width = 1f, float height = 1f)
        {
            SetSize(width, height);
        }
#endif
    }
}