using UnityEngine;

namespace DucMinh
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererSetOrder : MonoBehaviour, ISetOrder
    {
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetOrder(string sortingLayer, int orderInLayer)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingLayerName = sortingLayer;
                _spriteRenderer.sortingOrder = orderInLayer;
            }
        }
    }
}
