using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public class LegacyInputSystem : IInputProvider
    {
        private readonly KeyCode[] _keyMap;

        // ── Mouse delta cache (computed once per frame) ────────
        private Vector2 _prevMousePos;
        private Vector2 _mouseDelta;
        private int _deltaFrame = -1;

        public LegacyInputSystem()
        {
            var keyCount = Helper.GetEnumCount<InputKey>();
            _keyMap = new KeyCode[keyCount];

            for (int i = 0; i < keyCount; i++)
            {
                _keyMap[i] = InputKeyMap.ToLegacyKey((InputKey)i);
            }
        }

        // ── Keyboard & Mouse Buttons ───────────────────────────

        public bool GetKeyDown(InputKey key) => Input.GetKeyDown(_keyMap[(int)key]);

        public bool GetKeyUp(InputKey key) => Input.GetKeyUp(_keyMap[(int)key]);

        public bool GetKey(InputKey key) => Input.GetKey(_keyMap[(int)key]);

        public bool GetAnyKeyDown(IEnumerable<InputKey> keys)
        {
            foreach (var key in keys)
                if (Input.GetKeyDown(_keyMap[(int)key])) return true;
            return false;
        }

        public bool GetAnyKey(IEnumerable<InputKey> keys)
        {
            foreach (var key in keys)
                if (Input.GetKey(_keyMap[(int)key])) return true;
            return false;
        }


        public Vector2 MousePosition => Input.mousePosition;

        public Vector2 MouseDelta
        {
            get
            {
                var frame = Time.frameCount;
                if (_deltaFrame == frame) return _mouseDelta;
                Vector2 cur = Input.mousePosition;
                _mouseDelta = cur - _prevMousePos;
                _prevMousePos = cur;
                _deltaFrame = frame;
                return _mouseDelta;
            }
        }

        public Vector2 ScrollDelta => Input.mouseScrollDelta;

        // ── Touch ─────────────────────────────────────────────

        public int TouchCount => Input.touchCount;

        public InputTouch GetTouch(int index)
        {
            var t = Input.GetTouch(index);
            return new InputTouch
            {
                FingerId = t.fingerId,
                Position = t.position,
                DeltaPosition = t.deltaPosition,
                Phase = ConvertPhase(t.phase),
            };
        }

        private static InputTouchPhase ConvertPhase(TouchPhase phase) => phase switch
        {
            TouchPhase.Began => InputTouchPhase.Began,
            TouchPhase.Moved => InputTouchPhase.Moved,
            TouchPhase.Stationary => InputTouchPhase.Stationary,
            TouchPhase.Ended => InputTouchPhase.Ended,
            TouchPhase.Canceled => InputTouchPhase.Canceled,
            _ => InputTouchPhase.None,
        };
    }
}