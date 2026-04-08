using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using NISTouch = UnityEngine.InputSystem.TouchPhase;

namespace DucMinh
{
    public class NewInputSystem : IInputProvider
    {
        private readonly Key[] _keyMap;
        private Keyboard _keyboard => Keyboard.current;

        private static readonly InputKey _mouseFirst = InputKey.MouseLeft;

        public NewInputSystem()
        {
            var keyCount = Helper.GetEnumCount<InputKey>();
            _keyMap = new Key[keyCount];

            for (int i = 0; i < keyCount; i++)
            {
                _keyMap[i] = InputKeyMap.ToNewKey((InputKey)i);
            }
            
            if (!EnhancedTouchSupport.enabled)
                EnhancedTouchSupport.Enable();
        }

        // ── Internal helpers ───────────────────────────────────

        private static bool IsMouseButton(InputKey key) => key >= _mouseFirst;

        /// <summary>Returns the ButtonControl for a mouse InputKey, or null.</summary>
        private static ButtonControl GetMouseButton(InputKey key)
        {
            var mouse = Mouse.current;
            if (mouse == null) return null;
            return key switch
            {
                InputKey.MouseLeft => mouse.leftButton,
                InputKey.MouseRight => mouse.rightButton,
                InputKey.MouseMiddle => mouse.middleButton,
                InputKey.MouseButton3 => mouse.backButton,
                InputKey.MouseButton4 => mouse.forwardButton,
                _ => null,
            };
        }

        // ── Keyboard & Mouse Buttons ───────────────────────────

        public bool GetKeyDown(InputKey key)
        {
            if (IsMouseButton(key))
                return GetMouseButton(key)?.wasPressedThisFrame ?? false;
            return _keyboard[_keyMap[(int)key]].wasPressedThisFrame;
        }

        public bool GetKeyUp(InputKey key)
        {
            if (IsMouseButton(key))
                return GetMouseButton(key)?.wasReleasedThisFrame ?? false;
            return _keyboard[_keyMap[(int)key]].wasReleasedThisFrame;
        }

        public bool GetKey(InputKey key)
        {
            if (IsMouseButton(key))
                return GetMouseButton(key)?.isPressed ?? false;
            return _keyboard[_keyMap[(int)key]].isPressed;
        }

        public bool GetAnyKeyDown(IEnumerable<InputKey> keys)
        {
            foreach (var k in keys)
                if (GetKeyDown(k)) return true;
            return false;
        }
        
        public bool GetAnyKey(IEnumerable<InputKey> keys)
        {
            foreach (var key in keys)
                if (GetKey(key)) return true;
            return false;
        }

        // ── Mouse Pointer ──────────────────────────────────────

        public Vector2 MousePosition
        {
            get
            {
                var m = Mouse.current;
                return m != null ? m.position.ReadValue() : Vector2.zero;
            }
        }

        public Vector2 MouseDelta
        {
            get
            {
                var m = Mouse.current;
                return m != null ? m.delta.ReadValue() : Vector2.zero;
            }
        }

        public Vector2 ScrollDelta
        {
            get
            {
                var m = Mouse.current;
                // Unity's conventional "notch" scale (120 px = 1 notch).
                return m != null ? m.scroll.ReadValue() / 120f : Vector2.zero;
            }
        }

        // ── Touch ─────────────────────────────────────────────

        public int TouchCount => Touch.activeTouches.Count;

        public InputTouch GetTouch(int index)
        {
            var t = Touch.activeTouches[index];
            return new InputTouch
            {
                FingerId = t.finger.index,
                Position = t.screenPosition,
                DeltaPosition = t.delta,
                Phase = ConvertPhase(t.phase),
            };
        }

        private static InputTouchPhase ConvertPhase(NISTouch phase) => phase switch
        {
            NISTouch.Began => InputTouchPhase.Began,
            NISTouch.Moved => InputTouchPhase.Moved,
            NISTouch.Stationary => InputTouchPhase.Stationary,
            NISTouch.Ended => InputTouchPhase.Ended,
            NISTouch.Canceled => InputTouchPhase.Canceled,
            _ => InputTouchPhase.None,
        };
    }
}