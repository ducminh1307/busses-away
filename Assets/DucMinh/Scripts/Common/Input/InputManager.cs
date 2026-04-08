using UnityEngine;

namespace DucMinh
{
    public class InputManager : SingletonBehavior<InputManager>
    {
        private IInputProvider _inputProvider = new NewInputSystem();

        public IInputProvider InputProvider
        {
            get => _inputProvider;
            private set => _inputProvider = value;
        }

        public void SetProvider(IInputProvider provider)
        {
            InputProvider = provider;
        }

        public static void Init()
        {
            Instance.InitProvider();
        }

        private void InitProvider()
        {
            InputProvider = new NewInputSystem();
        }

        public static bool GetKeyDown(InputKey key) => Instance.InputProvider.GetKeyDown(key);

        public static bool GetKeyUp(InputKey key) => Instance.InputProvider.GetKeyUp(key);

        public static bool GetKey(InputKey key) => Instance.InputProvider.GetKey(key);

        public static bool GetAnyKeyDown(params InputKey[] keys) => Instance.InputProvider.GetAnyKeyDown(keys);
        public static bool GetAnyKey(params InputKey[] keys) => Instance.InputProvider.GetAnyKey(keys);

        public static Vector2 MousePosition => Instance.InputProvider.MousePosition;

        public static Vector2 MouseDelta => Instance.InputProvider.MouseDelta;

        public static Vector2 ScrollDelta => Instance.InputProvider.ScrollDelta;

        public static bool MouseLeftDown => Instance.InputProvider.GetKeyDown(InputKey.MouseLeft);
        public static bool MouseLeftUp => Instance.InputProvider.GetKeyUp(InputKey.MouseLeft);
        public static bool MouseLeft => Instance.InputProvider.GetKey(InputKey.MouseLeft);

        public static bool MouseRightDown => Instance.InputProvider.GetKeyDown(InputKey.MouseRight);
        public static bool MouseRightUp => Instance.InputProvider.GetKeyUp(InputKey.MouseRight);
        public static bool MouseRight => Instance.InputProvider.GetKey(InputKey.MouseRight);

        public static bool MouseMiddleDown => Instance.InputProvider.GetKeyDown(InputKey.MouseMiddle);
        public static bool MouseMiddleUp => Instance.InputProvider.GetKeyUp(InputKey.MouseMiddle);
        public static bool MouseMiddle => Instance.InputProvider.GetKey(InputKey.MouseMiddle);

        public static int TouchCount => Instance.InputProvider.TouchCount;

        public static InputTouch GetTouch(int index) => Instance.InputProvider.GetTouch(index);

        public static Vector2 PointerPosition
        {
            get
            {
                var p = Instance.InputProvider;
                return p.TouchCount > 0 ? p.GetTouch(0).Position : p.MousePosition;
            }
        }

        public static bool PointerDown
        {
            get
            {
                var p = Instance.InputProvider;
                if (p.TouchCount > 0 && p.GetTouch(0).Phase == InputTouchPhase.Began)
                    return true;
                return p.GetKeyDown(InputKey.MouseLeft);
            }
        }

        public static bool PointerUp
        {
            get
            {
                var p = Instance.InputProvider;
                if (p.TouchCount <= 0) return p.GetKeyUp(InputKey.MouseLeft);
                var phase = p.GetTouch(0).Phase;
                return phase is InputTouchPhase.Ended or InputTouchPhase.Canceled || p.GetKeyUp(InputKey.MouseLeft);
            }
        }
    }
}