using DucMinh.MultiProperties;
using UnityEngine;

namespace DucMinh
{
    public class PaddingSprite: MonoBehaviour, ISetSize
    {
        [SerializeField] private bool keepInnerCenter = true;
        [SerializeField] private Padding padding;
        
        private SpriteRenderer _spriteRenderer;

        public void SetSize(float width, float height)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (_spriteRenderer.IsNullObject()) return;
            
            //Set draw mode to sliced
            _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            //Set size with padding
            var outerSize = new Vector2(
                width + padding.Left + padding.Right, 
                height + padding.Top + padding.Bottom);
            _spriteRenderer.size = outerSize;

            if (keepInnerCenter)
            {
                //Set position with padding
                var pos = transform.localPosition;
                pos = new Vector2(
                    pos.x + (padding.Right - padding.Left) * .5f, 
                    pos.y + (padding.Top - padding.Bottom) * .5f);
                transform.localPosition = pos;
            }
        }
    }
}