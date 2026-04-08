using UnityEngine;

namespace DucMinh
{
    /// <summary>
    /// Shared touch data struct – bridges Legacy (UnityEngine.Touch)
    /// and New Input System (EnhancedTouch.Touch).
    /// </summary>
    public struct InputTouch
    {
        public int      FingerId;
        public Vector2  Position;
        public Vector2  DeltaPosition;
        public InputTouchPhase Phase;
    }

    public enum InputTouchPhase
    {
        None,
        Began,
        Moved,
        Stationary,
        Ended,
        Canceled,
    }
}
