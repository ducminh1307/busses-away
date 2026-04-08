using UnityEngine;
using UnityEngine.EventSystems;
#if DOTWEEN
using DG.Tweening;
#endif

namespace DucMinh
{
    public class ScaleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float targetScale = .95f;
#if DOTWEEN
        [SerializeField] private float animationDuration = 0.1f;
        [SerializeField] private Ease easeType = Ease.OutQuad;
#endif
        
        private Vector3 initialScale; 

        private void Start()
        {
            initialScale = transform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var target = initialScale * targetScale;
#if DOTWEEN
            transform.DOScale(target, animationDuration)
                .SetEase(easeType);
#else
            transform.SetScale(target);
#endif
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if DOTWEEN
                transform.DOScale(initialScale, animationDuration)
                    .SetEase(easeType);
#else
                transform.localScale = initialScale;
#endif
        }
#if DOTWEEN
        void OnDestroy()
        {
            transform.DOKill();
        }
#endif
    }
}