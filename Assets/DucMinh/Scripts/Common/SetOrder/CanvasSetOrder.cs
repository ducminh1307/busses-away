using UnityEngine;

namespace DucMinh
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasSetOrder : MonoBehaviour, ISetOrder
    {
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void SetOrder(string sortingLayer, int orderInLayer)
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }

            if (_canvas != null)
            {
                _canvas.sortingLayerName = sortingLayer;
                _canvas.sortingOrder = orderInLayer;
            }
        }
    }
}
