namespace DucMinh
{
    public enum InputKey
    {
        // ── Alpha ──────────────────────────────────────────
        A, B, C, D, E, F, G, H, I, J, K, L, M,
        N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

        // ── Digit (top row) ───────────────────────────────
        Alpha0, Alpha1, Alpha2, Alpha3, Alpha4,
        Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,

        // ── Numpad ────────────────────────────────────────
        Numpad0, Numpad1, Numpad2, Numpad3, Numpad4,
        Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
        NumpadPlus, NumpadMinus, NumpadMultiply, NumpadDivide,
        NumpadPeriod, NumpadEnter,

        // ── Function ──────────────────────────────────────
        F1, F2, F3, F4, F5, F6,
        F7, F8, F9, F10, F11, F12,

        // ── Arrow ─────────────────────────────────────────
        UpArrow, DownArrow, LeftArrow, RightArrow,

        // ── Modifier ──────────────────────────────────────
        LeftShift, RightShift,
        LeftControl, RightControl,
        LeftAlt, RightAlt,
        LeftCommand, RightCommand,   // macOS / Windows key

        // ── Navigation ────────────────────────────────────
        Insert, Delete, Home, End, PageUp, PageDown,

        // ── Whitespace / Control ──────────────────────────
        Space, Return, Backspace, Tab, Escape, CapsLock,

        // ── Punctuation / Symbol ──────────────────────────
        Backquote,          // `  ~
        Minus,              // -  _
        Equals,             // =  +
        LeftBracket,        // [  {
        RightBracket,       // ]  }
        Backslash,          // \  |
        Semicolon,          // ;  :
        Quote,              // '  "
        Comma,              // ,  <
        Period,             // .  >
        Slash,              // /  ?

        // ── Media / Extra ─────────────────────────────────
        PrintScreen, ScrollLock, Pause,
        NumLock,
        ContextMenu,

        // ── Mouse Buttons ─────────────────────────────────
        // NOTE: Keep mouse entries at the end so keyboard indices stay stable.
        MouseLeft,          // Primary / Left click
        MouseRight,         // Secondary / Right click
        MouseMiddle,        // Middle / Scroll-wheel button
        MouseButton3,       // Extra button 3
        MouseButton4,       // Extra button 4
    }
}