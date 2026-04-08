using UnityEngine;
using UnityEngine.EventSystems;

namespace DucMinh
{
    public class FloatingJoystick : Joystick
    {
        private RectTransform activationArea;
        [SerializeField] private bool hideWhenInactive = true;

        protected override void Start()
        {
            base.Start();
            activationArea = GetComponent<RectTransform>();
            if (hideWhenInactive)
            {
                background.SetShow(false);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (activationArea != null && RectTransformUtility.RectangleContainsScreenPoint(activationArea, eventData.position))
            {
                background.SetShow(true);

                background.position = eventData.position;
                startPos = background.position;
                
                base.OnDrag(eventData);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (hideWhenInactive)
            {
                background.SetShow(false);
            }
        }
    }
}