using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public static class UserInput
    {
        private static GameObject _uiInputBlocker;

        private static GameObject _UIInputBlocker
        {
            get
            {
                if (_uiInputBlocker.IsNullObject())
                {
                    _uiInputBlocker = "Block Canvas".Create();
                    
                    var canvas = _uiInputBlocker.GetOrAddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.SetCanvasSortingLayerName("UI");
                    canvas.SetCanvasSortingOrder(100);
                    
                    _uiInputBlocker.GetOrAddComponent<CanvasScaler>();
                    _uiInputBlocker.GetOrAddComponent<GraphicRaycaster>();
                    var canvasGroup = _UIInputBlocker.GetOrAddComponent<CanvasGroup>();

                    var imageBlock = "Block".Create(_uiInputBlocker.transform);
                    
                    var rect = imageBlock.GetOrAddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    
                    var image = imageBlock.GetOrAddComponent<Image>();
                    image.color = Color.clear;
                    image.raycastTarget = true;
                }
                return _uiInputBlocker;
            }
        }
        
        private static bool _enabled = true;
        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                
                _enabled = value;
                if (_UIInputBlocker.TryGetComponent(out CanvasGroup canvasGroup))
                {
                    canvasGroup.blocksRaycasts = !value;
                    canvasGroup.alpha = !value ? 1f : 0f;
                }
            }
        }
    }
}