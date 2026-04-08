using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DucMinh
{
    public enum AxisOption { Both, Horizontal, Vertical }
    
    public class Joystick: MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private Vector2 inputVector;
    
        [SerializeField] protected AxisOption axis = AxisOption.Both;
        [SerializeField] protected RectTransform background;
        [SerializeField] protected RectTransform handle;
    
        protected Vector2 startPos;
        private float radius;

        protected virtual void Start()
        {
            startPos = background.position;
            radius = background.rect.size.x / 2f;
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            var position = eventData.position;
            var diff = position - startPos;
            
            diff = Vector2.ClampMagnitude(diff, radius);
            handle.position = startPos + diff;

            inputVector = diff / radius;
             
            switch (axis)
            {
                case AxisOption.Horizontal:
                    inputVector = new Vector2(inputVector.x, 0);
                    break;
                case AxisOption.Vertical:
                    inputVector = new Vector2(0, inputVector.y);
                    break;
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            handle.position = startPos;
        }

        public float Horizontal()
        {
            return inputVector.x;
        }

        public float Vertical()
        {
            return inputVector.y;
        }

        public Vector2 Direction => inputVector;
    }
}