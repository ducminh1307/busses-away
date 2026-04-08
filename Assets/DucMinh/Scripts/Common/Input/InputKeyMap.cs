using UnityEngine;
using UnityEngine.InputSystem;

namespace DucMinh
{
    public static class InputKeyMap
    {
        private static readonly KeyCode[] LegacyMap;
        private static readonly Key[] NewMap;

        static InputKeyMap()
        {
            var keyCount = Helper.GetEnumCount<InputKey>();
            LegacyMap = new KeyCode[keyCount];
            NewMap = new Key[keyCount];

            // ── Alpha ──────────────────────────────────────────
            LegacyMap[(int)InputKey.A] = KeyCode.A;
            LegacyMap[(int)InputKey.B] = KeyCode.B;
            LegacyMap[(int)InputKey.C] = KeyCode.C;
            LegacyMap[(int)InputKey.D] = KeyCode.D;
            LegacyMap[(int)InputKey.E] = KeyCode.E;
            LegacyMap[(int)InputKey.F] = KeyCode.F;
            LegacyMap[(int)InputKey.G] = KeyCode.G;
            LegacyMap[(int)InputKey.H] = KeyCode.H;
            LegacyMap[(int)InputKey.I] = KeyCode.I;
            LegacyMap[(int)InputKey.J] = KeyCode.J;
            LegacyMap[(int)InputKey.K] = KeyCode.K;
            LegacyMap[(int)InputKey.L] = KeyCode.L;
            LegacyMap[(int)InputKey.M] = KeyCode.M;
            LegacyMap[(int)InputKey.N] = KeyCode.N;
            LegacyMap[(int)InputKey.O] = KeyCode.O;
            LegacyMap[(int)InputKey.P] = KeyCode.P;
            LegacyMap[(int)InputKey.Q] = KeyCode.Q;
            LegacyMap[(int)InputKey.R] = KeyCode.R;
            LegacyMap[(int)InputKey.S] = KeyCode.S;
            LegacyMap[(int)InputKey.T] = KeyCode.T;
            LegacyMap[(int)InputKey.U] = KeyCode.U;
            LegacyMap[(int)InputKey.V] = KeyCode.V;
            LegacyMap[(int)InputKey.W] = KeyCode.W;
            LegacyMap[(int)InputKey.X] = KeyCode.X;
            LegacyMap[(int)InputKey.Y] = KeyCode.Y;
            LegacyMap[(int)InputKey.Z] = KeyCode.Z;

            NewMap[(int)InputKey.A] = Key.A;
            NewMap[(int)InputKey.B] = Key.B;
            NewMap[(int)InputKey.C] = Key.C;
            NewMap[(int)InputKey.D] = Key.D;
            NewMap[(int)InputKey.E] = Key.E;
            NewMap[(int)InputKey.F] = Key.F;
            NewMap[(int)InputKey.G] = Key.G;
            NewMap[(int)InputKey.H] = Key.H;
            NewMap[(int)InputKey.I] = Key.I;
            NewMap[(int)InputKey.J] = Key.J;
            NewMap[(int)InputKey.K] = Key.K;
            NewMap[(int)InputKey.L] = Key.L;
            NewMap[(int)InputKey.M] = Key.M;
            NewMap[(int)InputKey.N] = Key.N;
            NewMap[(int)InputKey.O] = Key.O;
            NewMap[(int)InputKey.P] = Key.P;
            NewMap[(int)InputKey.Q] = Key.Q;
            NewMap[(int)InputKey.R] = Key.R;
            NewMap[(int)InputKey.S] = Key.S;
            NewMap[(int)InputKey.T] = Key.T;
            NewMap[(int)InputKey.U] = Key.U;
            NewMap[(int)InputKey.V] = Key.V;
            NewMap[(int)InputKey.W] = Key.W;
            NewMap[(int)InputKey.X] = Key.X;
            NewMap[(int)InputKey.Y] = Key.Y;
            NewMap[(int)InputKey.Z] = Key.Z;

            // ── Digit (top row) ───────────────────────────────
            LegacyMap[(int)InputKey.Alpha0] = KeyCode.Alpha0;
            LegacyMap[(int)InputKey.Alpha1] = KeyCode.Alpha1;
            LegacyMap[(int)InputKey.Alpha2] = KeyCode.Alpha2;
            LegacyMap[(int)InputKey.Alpha3] = KeyCode.Alpha3;
            LegacyMap[(int)InputKey.Alpha4] = KeyCode.Alpha4;
            LegacyMap[(int)InputKey.Alpha5] = KeyCode.Alpha5;
            LegacyMap[(int)InputKey.Alpha6] = KeyCode.Alpha6;
            LegacyMap[(int)InputKey.Alpha7] = KeyCode.Alpha7;
            LegacyMap[(int)InputKey.Alpha8] = KeyCode.Alpha8;
            LegacyMap[(int)InputKey.Alpha9] = KeyCode.Alpha9;

            NewMap[(int)InputKey.Alpha0] = Key.Digit0;
            NewMap[(int)InputKey.Alpha1] = Key.Digit1;
            NewMap[(int)InputKey.Alpha2] = Key.Digit2;
            NewMap[(int)InputKey.Alpha3] = Key.Digit3;
            NewMap[(int)InputKey.Alpha4] = Key.Digit4;
            NewMap[(int)InputKey.Alpha5] = Key.Digit5;
            NewMap[(int)InputKey.Alpha6] = Key.Digit6;
            NewMap[(int)InputKey.Alpha7] = Key.Digit7;
            NewMap[(int)InputKey.Alpha8] = Key.Digit8;
            NewMap[(int)InputKey.Alpha9] = Key.Digit9;

            // ── Numpad ────────────────────────────────────────
            LegacyMap[(int)InputKey.Numpad0]        = KeyCode.Keypad0;
            LegacyMap[(int)InputKey.Numpad1]        = KeyCode.Keypad1;
            LegacyMap[(int)InputKey.Numpad2]        = KeyCode.Keypad2;
            LegacyMap[(int)InputKey.Numpad3]        = KeyCode.Keypad3;
            LegacyMap[(int)InputKey.Numpad4]        = KeyCode.Keypad4;
            LegacyMap[(int)InputKey.Numpad5]        = KeyCode.Keypad5;
            LegacyMap[(int)InputKey.Numpad6]        = KeyCode.Keypad6;
            LegacyMap[(int)InputKey.Numpad7]        = KeyCode.Keypad7;
            LegacyMap[(int)InputKey.Numpad8]        = KeyCode.Keypad8;
            LegacyMap[(int)InputKey.Numpad9]        = KeyCode.Keypad9;
            LegacyMap[(int)InputKey.NumpadPlus]     = KeyCode.KeypadPlus;
            LegacyMap[(int)InputKey.NumpadMinus]    = KeyCode.KeypadMinus;
            LegacyMap[(int)InputKey.NumpadMultiply] = KeyCode.KeypadMultiply;
            LegacyMap[(int)InputKey.NumpadDivide]   = KeyCode.KeypadDivide;
            LegacyMap[(int)InputKey.NumpadPeriod]   = KeyCode.KeypadPeriod;
            LegacyMap[(int)InputKey.NumpadEnter]    = KeyCode.KeypadEnter;

            NewMap[(int)InputKey.Numpad0]        = Key.Numpad0;
            NewMap[(int)InputKey.Numpad1]        = Key.Numpad1;
            NewMap[(int)InputKey.Numpad2]        = Key.Numpad2;
            NewMap[(int)InputKey.Numpad3]        = Key.Numpad3;
            NewMap[(int)InputKey.Numpad4]        = Key.Numpad4;
            NewMap[(int)InputKey.Numpad5]        = Key.Numpad5;
            NewMap[(int)InputKey.Numpad6]        = Key.Numpad6;
            NewMap[(int)InputKey.Numpad7]        = Key.Numpad7;
            NewMap[(int)InputKey.Numpad8]        = Key.Numpad8;
            NewMap[(int)InputKey.Numpad9]        = Key.Numpad9;
            NewMap[(int)InputKey.NumpadPlus]     = Key.NumpadPlus;
            NewMap[(int)InputKey.NumpadMinus]    = Key.NumpadMinus;
            NewMap[(int)InputKey.NumpadMultiply] = Key.NumpadMultiply;
            NewMap[(int)InputKey.NumpadDivide]   = Key.NumpadDivide;
            NewMap[(int)InputKey.NumpadPeriod]   = Key.NumpadPeriod;
            NewMap[(int)InputKey.NumpadEnter]    = Key.NumpadEnter;

            // ── Function ──────────────────────────────────────
            LegacyMap[(int)InputKey.F1]  = KeyCode.F1;
            LegacyMap[(int)InputKey.F2]  = KeyCode.F2;
            LegacyMap[(int)InputKey.F3]  = KeyCode.F3;
            LegacyMap[(int)InputKey.F4]  = KeyCode.F4;
            LegacyMap[(int)InputKey.F5]  = KeyCode.F5;
            LegacyMap[(int)InputKey.F6]  = KeyCode.F6;
            LegacyMap[(int)InputKey.F7]  = KeyCode.F7;
            LegacyMap[(int)InputKey.F8]  = KeyCode.F8;
            LegacyMap[(int)InputKey.F9]  = KeyCode.F9;
            LegacyMap[(int)InputKey.F10] = KeyCode.F10;
            LegacyMap[(int)InputKey.F11] = KeyCode.F11;
            LegacyMap[(int)InputKey.F12] = KeyCode.F12;

            NewMap[(int)InputKey.F1]  = Key.F1;
            NewMap[(int)InputKey.F2]  = Key.F2;
            NewMap[(int)InputKey.F3]  = Key.F3;
            NewMap[(int)InputKey.F4]  = Key.F4;
            NewMap[(int)InputKey.F5]  = Key.F5;
            NewMap[(int)InputKey.F6]  = Key.F6;
            NewMap[(int)InputKey.F7]  = Key.F7;
            NewMap[(int)InputKey.F8]  = Key.F8;
            NewMap[(int)InputKey.F9]  = Key.F9;
            NewMap[(int)InputKey.F10] = Key.F10;
            NewMap[(int)InputKey.F11] = Key.F11;
            NewMap[(int)InputKey.F12] = Key.F12;

            // ── Arrow ─────────────────────────────────────────
            LegacyMap[(int)InputKey.UpArrow]    = KeyCode.UpArrow;
            LegacyMap[(int)InputKey.DownArrow]  = KeyCode.DownArrow;
            LegacyMap[(int)InputKey.LeftArrow]  = KeyCode.LeftArrow;
            LegacyMap[(int)InputKey.RightArrow] = KeyCode.RightArrow;

            NewMap[(int)InputKey.UpArrow]    = Key.UpArrow;
            NewMap[(int)InputKey.DownArrow]  = Key.DownArrow;
            NewMap[(int)InputKey.LeftArrow]  = Key.LeftArrow;
            NewMap[(int)InputKey.RightArrow] = Key.RightArrow;

            // ── Modifier ──────────────────────────────────────
            LegacyMap[(int)InputKey.LeftShift]    = KeyCode.LeftShift;
            LegacyMap[(int)InputKey.RightShift]   = KeyCode.RightShift;
            LegacyMap[(int)InputKey.LeftControl]  = KeyCode.LeftControl;
            LegacyMap[(int)InputKey.RightControl] = KeyCode.RightControl;
            LegacyMap[(int)InputKey.LeftAlt]      = KeyCode.LeftAlt;
            LegacyMap[(int)InputKey.RightAlt]     = KeyCode.RightAlt;
            LegacyMap[(int)InputKey.LeftCommand]  = KeyCode.LeftCommand;
            LegacyMap[(int)InputKey.RightCommand] = KeyCode.RightCommand;

            NewMap[(int)InputKey.LeftShift]    = Key.LeftShift;
            NewMap[(int)InputKey.RightShift]   = Key.RightShift;
            NewMap[(int)InputKey.LeftControl]  = Key.LeftCtrl;
            NewMap[(int)InputKey.RightControl] = Key.RightCtrl;
            NewMap[(int)InputKey.LeftAlt]      = Key.LeftAlt;
            NewMap[(int)InputKey.RightAlt]     = Key.RightAlt;
            NewMap[(int)InputKey.LeftCommand]  = Key.LeftMeta;
            NewMap[(int)InputKey.RightCommand] = Key.RightMeta;

            // ── Navigation ────────────────────────────────────
            LegacyMap[(int)InputKey.Insert]   = KeyCode.Insert;
            LegacyMap[(int)InputKey.Delete]   = KeyCode.Delete;
            LegacyMap[(int)InputKey.Home]     = KeyCode.Home;
            LegacyMap[(int)InputKey.End]      = KeyCode.End;
            LegacyMap[(int)InputKey.PageUp]   = KeyCode.PageUp;
            LegacyMap[(int)InputKey.PageDown] = KeyCode.PageDown;

            NewMap[(int)InputKey.Insert]   = Key.Insert;
            NewMap[(int)InputKey.Delete]   = Key.Delete;
            NewMap[(int)InputKey.Home]     = Key.Home;
            NewMap[(int)InputKey.End]      = Key.End;
            NewMap[(int)InputKey.PageUp]   = Key.PageUp;
            NewMap[(int)InputKey.PageDown] = Key.PageDown;

            // ── Whitespace / Control ──────────────────────────
            LegacyMap[(int)InputKey.Space]     = KeyCode.Space;
            LegacyMap[(int)InputKey.Return]    = KeyCode.Return;
            LegacyMap[(int)InputKey.Backspace] = KeyCode.Backspace;
            LegacyMap[(int)InputKey.Tab]       = KeyCode.Tab;
            LegacyMap[(int)InputKey.Escape]    = KeyCode.Escape;
            LegacyMap[(int)InputKey.CapsLock]  = KeyCode.CapsLock;

            NewMap[(int)InputKey.Space]     = Key.Space;
            NewMap[(int)InputKey.Return]    = Key.Enter;
            NewMap[(int)InputKey.Backspace] = Key.Backspace;
            NewMap[(int)InputKey.Tab]       = Key.Tab;
            NewMap[(int)InputKey.Escape]    = Key.Escape;
            NewMap[(int)InputKey.CapsLock]  = Key.CapsLock;

            // ── Punctuation / Symbol ──────────────────────────
            LegacyMap[(int)InputKey.Backquote]   = KeyCode.BackQuote;
            LegacyMap[(int)InputKey.Minus]       = KeyCode.Minus;
            LegacyMap[(int)InputKey.Equals]      = KeyCode.Equals;
            LegacyMap[(int)InputKey.LeftBracket] = KeyCode.LeftBracket;
            LegacyMap[(int)InputKey.RightBracket]= KeyCode.RightBracket;
            LegacyMap[(int)InputKey.Backslash]   = KeyCode.Backslash;
            LegacyMap[(int)InputKey.Semicolon]   = KeyCode.Semicolon;
            LegacyMap[(int)InputKey.Quote]       = KeyCode.Quote;
            LegacyMap[(int)InputKey.Comma]       = KeyCode.Comma;
            LegacyMap[(int)InputKey.Period]      = KeyCode.Period;
            LegacyMap[(int)InputKey.Slash]       = KeyCode.Slash;

            NewMap[(int)InputKey.Backquote]   = Key.Backquote;
            NewMap[(int)InputKey.Minus]       = Key.Minus;
            NewMap[(int)InputKey.Equals]      = Key.Equals;
            NewMap[(int)InputKey.LeftBracket] = Key.LeftBracket;
            NewMap[(int)InputKey.RightBracket]= Key.RightBracket;
            NewMap[(int)InputKey.Backslash]   = Key.Backslash;
            NewMap[(int)InputKey.Semicolon]   = Key.Semicolon;
            NewMap[(int)InputKey.Quote]       = Key.Quote;
            NewMap[(int)InputKey.Comma]       = Key.Comma;
            NewMap[(int)InputKey.Period]      = Key.Period;
            NewMap[(int)InputKey.Slash]       = Key.Slash;

            // ── Media / Extra ─────────────────────────────────
            LegacyMap[(int)InputKey.PrintScreen] = KeyCode.Print;
            LegacyMap[(int)InputKey.ScrollLock]  = KeyCode.ScrollLock;
            LegacyMap[(int)InputKey.Pause]       = KeyCode.Pause;
            LegacyMap[(int)InputKey.NumLock]     = KeyCode.Numlock;
            LegacyMap[(int)InputKey.ContextMenu] = KeyCode.Menu;

            NewMap[(int)InputKey.PrintScreen] = Key.PrintScreen;
            NewMap[(int)InputKey.ScrollLock]  = Key.ScrollLock;
            NewMap[(int)InputKey.Pause]       = Key.Pause;
            NewMap[(int)InputKey.NumLock]     = Key.NumLock;
            NewMap[(int)InputKey.ContextMenu] = Key.ContextMenu;

            // ── Mouse Buttons ─────────────────────────────────
            LegacyMap[(int)InputKey.MouseLeft]    = KeyCode.Mouse0;
            LegacyMap[(int)InputKey.MouseRight]   = KeyCode.Mouse1;
            LegacyMap[(int)InputKey.MouseMiddle]  = KeyCode.Mouse2;
            LegacyMap[(int)InputKey.MouseButton3] = KeyCode.Mouse3;
            LegacyMap[(int)InputKey.MouseButton4] = KeyCode.Mouse4;

            // New Input System: mouse xử lý qua Mouse.current, nên để None
            NewMap[(int)InputKey.MouseLeft]    = Key.None;
            NewMap[(int)InputKey.MouseRight]   = Key.None;
            NewMap[(int)InputKey.MouseMiddle]  = Key.None;
            NewMap[(int)InputKey.MouseButton3] = Key.None;
            NewMap[(int)InputKey.MouseButton4] = Key.None;
        }

        public static KeyCode ToLegacyKey(InputKey key)
            => LegacyMap[(int)key];

        public static Key ToNewKey(InputKey key)
            => NewMap[(int)key];
    }
}