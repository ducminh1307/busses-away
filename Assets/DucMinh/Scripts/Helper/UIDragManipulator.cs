using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

namespace DucMinh
{
    public class UIDragManipulator : PointerManipulator
    {
        private Vector2 _startPointerPosition;
        private Vector2 _startElementPosition;
        private bool _enabled;
        private VisualElement _root;

        public event Action<VisualElement> OnDragStart;
        public event Action<VisualElement> OnDragMove;
        public event Action<VisualElement, Vector2> OnDropped;

        public float SnapSize { get; set; } = 0;
        public List<VisualElement> SnapTargets { get; set; } = new List<VisualElement>();
        public float SnapThreshold { get; set; } = 50f;

        public UIDragManipulator(VisualElement root = null)
        {
            _root = root;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_enabled) return;

            _startPointerPosition = evt.position;
            _startElementPosition = new Vector2(target.layout.x, target.layout.y);

            target.style.position = Position.Absolute;

            target.CapturePointer(evt.pointerId);
            _enabled = true;
            OnDragStart?.Invoke(target);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_enabled || !target.HasPointerCapture(evt.pointerId)) return;

            Vector2 pointerDelta = (Vector2)evt.position - _startPointerPosition;
            Vector2 newPos = _startElementPosition + pointerDelta;

            if (SnapSize > 0)
            {
                newPos.x = Mathf.Round(newPos.x / SnapSize) * SnapSize;
                newPos.y = Mathf.Round(newPos.y / SnapSize) * SnapSize;
            }

            target.style.left = newPos.x;
            target.style.top = newPos.y;

            OnDragMove?.Invoke(target);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_enabled || !target.HasPointerCapture(evt.pointerId)) return;

            _enabled = false;
            target.ReleasePointer(evt.pointerId);

            Vector2 finalPos = new Vector2(target.resolvedStyle.left, target.resolvedStyle.top);

            if (SnapTargets != null && SnapTargets.Count > 0)
            {
                foreach (var snapTarget in SnapTargets)
                {
                    Vector2 targetCenter = snapTarget.worldBound.center;
                    Vector2 myCenter = target.worldBound.center;

                    if (Vector2.Distance(myCenter, targetCenter) < SnapThreshold)
                    {
                        var parentPos = target.parent.WorldToLocal(targetCenter);
                        target.style.left = parentPos.x - (target.layout.width / 2);
                        target.style.top = parentPos.y - (target.layout.height / 2);
                        finalPos = new Vector2(target.resolvedStyle.left, target.resolvedStyle.top);
                        break;
                    }
                }
            }

            OnDropped?.Invoke(target, finalPos);
            evt.StopPropagation();
        }
    }
}
