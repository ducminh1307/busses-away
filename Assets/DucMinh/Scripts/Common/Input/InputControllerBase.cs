#if UNITY_EDITOR
#define MOUSE_ENABLE
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace DucMinh
{
// #if NEW_INPUT_SYSTEM
//     public abstract class InputControllerBase<THit> : NewInputController<THit>
//     {
//     }
// #else
//     public abstract class InputControllerBase<THit> : OldInputController<THit>
//     {
//     }
// #endif
    public abstract class InputControllerBase<THit>
    {
        public Camera TargetCamera { get; set; }
        public IInputHandler<THit> Handler { get; set; }
        public bool Interactable { get; set; } = true;
        public float DragThreshold { get; set; } = 8f;
        public bool BlockOverUI { get; set; } = true;

        public bool IsDragging => _state.isDragging;
        public bool IsCaptured { get; private set; }
        public bool IsReleased { get; private set; } = true;
        public THit CurrentHit => _cachedHit;

        private (int id, Vector2 startScreen, Vector2 lastScreen, bool isActive, bool isDragging) _state;
        private PointerEventData _ped;
        private THit _cachedHit;
        private readonly ListPool<RaycastResult> _raycastResultPool = new();

        protected abstract THit RaycastAt(Vector2 screenPos);

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Camera EnsureCamera()
        {
            if (TargetCamera == null) TargetCamera = Context.MainCamera;
            return TargetCamera;
        }

        protected virtual bool ShouldBlockDueToUI(Vector2 screenPos)
        {
            if (!BlockOverUI) return false;
            if (EventSystem.current == null) return false;

            if (EventSystem.current.IsPointerOverGameObject()
#if MOUSE_ENABLE
                || EventSystem.current.IsPointerOverGameObject(-1)
#endif
               ) return true;

            _ped ??= new PointerEventData(EventSystem.current);
            _ped.position = screenPos;
            var results = _raycastResultPool.Get();
            EventSystem.current.RaycastAll(_ped, results);
            var hit = results.Count > 0;
            _raycastResultPool.Release(results);
            return hit;
        }
        
        public void Update()
        {
            if (!Interactable || Handler == null) return;

#if MOUSE_ENABLE
            if (Input.touchCount == 0)
            {
                HandleMouse();
                return;
            }
#endif
            HandleTouch();
        }

        // ── Touch ──────────────────────────────────────────────────────

        private void HandleTouch()
        {
            if (InputManager.TouchCount <= 0) return;

            var t = InputManager.GetTouch(0);
            switch (t.Phase)
            {
                case InputTouchPhase.Began:
                    if (ShouldBlockDueToUI(t.Position)) return;
                    Begin(t.FingerId, t.Position);
                    TryCapture();
                    break;
                case InputTouchPhase.Moved:
                case InputTouchPhase.Stationary:
                    if (!_state.isActive) return;
                    UpdatePointer(t.Position, isPressed: true);
                    if (_state.isDragging && IsCaptured) ForwardDrag();
                    break;
                case InputTouchPhase.Ended:
                case InputTouchPhase.Canceled:
                    if (!_state.isActive) return;
                    End(t.Position);
                    break;
            }
        }


#if MOUSE_ENABLE
        private void HandleMouse()
        {
            if (InputManager.MouseLeftDown)
            {
                var mousePosition = InputManager.MousePosition;
                if (ShouldBlockDueToUI(mousePosition)) return;
                Begin(-1, mousePosition);
                TryCapture();
            }
            else if (InputManager.MouseLeft)
            {
                var mousePosition = InputManager.MousePosition;
                if (!_state.isActive) return;
                UpdatePointer(mousePosition, isPressed: true);
                if (_state.isDragging && IsCaptured) ForwardDrag();
            }
            else if (InputManager.MouseLeftUp)
            {
                if (!_state.isActive) return;
                End(InputManager.MousePosition);
            }
        }
#endif
        
        private void Begin(int id, Vector2 screenPos)
        {
            _state = (id, screenPos, screenPos, true, false);
            IsCaptured = false;
            IsReleased = false;
            _cachedHit = RaycastAt(screenPos);
        }

        private void TryCapture()
        {
            IsCaptured = Handler.OnTap(_cachedHit);
        }

        private void UpdatePointer(Vector2 screenPos, bool isPressed)
        {
            _state.lastScreen = screenPos;
            _cachedHit = RaycastAt(screenPos);

            if (!_state.isDragging && isPressed)
            {
                float threshold = DragThreshold;
                if ((_state.lastScreen - _state.startScreen).sqrMagnitude >= threshold * threshold)
                    _state.isDragging = true;
            }
        }

        private void ForwardDrag()
        {
            if (!Handler.OnDrag(_cachedHit))
                CancelCapture();
        }

        private void End(Vector2 screenPos)
        {
            UpdatePointer(screenPos, isPressed: false);
            if (IsCaptured) Handler.OnRelease(_cachedHit);

            _state.isActive = false;
            _state.isDragging = false;
            IsCaptured = false;
            IsReleased = true;
        }

        private void CancelCapture()
        {
            IsCaptured = false;
            _state.isDragging = false;
        }
    }
}
