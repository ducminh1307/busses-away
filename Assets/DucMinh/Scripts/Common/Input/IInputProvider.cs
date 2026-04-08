using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public interface IInputProvider
    {
        // ── Keyboard & Mouse Buttons ───────────────────────────
        bool GetKeyDown(InputKey key);
        bool GetKeyUp(InputKey key);
        bool GetKey(InputKey key);
        bool GetAnyKeyDown(IEnumerable<InputKey> keys);
        bool GetAnyKey(IEnumerable<InputKey> keys);

        // ── Mouse Pointer ──────────────────────────────────────
        /// <summary>Current mouse position in screen pixels.</summary>
        Vector2 MousePosition { get; }

        /// <summary>Mouse movement delta since last frame in screen pixels.</summary>
        Vector2 MouseDelta { get; }

        /// <summary>Scroll-wheel delta. X = horizontal, Y = vertical.</summary>
        Vector2 ScrollDelta { get; }

        // ── Touch ─────────────────────────────────────────────
        /// <summary>Number of active touches this frame.</summary>
        int TouchCount { get; }

        /// <summary>Returns touch data for the given finger index (0-based).</summary>
        InputTouch GetTouch(int index);
    }
}