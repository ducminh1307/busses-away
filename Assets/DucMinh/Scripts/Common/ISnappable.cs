using UnityEngine;

namespace DucMinh
{
    /// <summary>
    /// Interface for objects that can be snapped to a snap target.
    /// This interface defines methods for getting the transform of the object,
    /// checking if it is currently snapped, getting the current snap target,
    /// trying to snap to a specified snap target, and unsnapping from the current snap target.
    /// </summary>
    public interface ISnappable
    {
        /// <summary>
        /// Get the transform of the snappable object.
        /// This is used to determine the position and orientation of the object in the world space.
        /// </summary>
        /// <returns>Transform of snappale object</returns>
        Transform GetTransform();
        /// <summary>
        /// Whether the object is currently snapped to a snap target.
        /// If true, the object is snapped to a snap target; otherwise, it is not snapped.
        /// The CurrentSnapTarget property can be used to get the snap target if IsSnapped is true.
        /// </summary>
        bool IsSnapped { get; }
        /// <summary>
        /// Get the current snap target that the object is snapped to.
        /// If the object is not snapped, this will return null.
        /// This property is only valid if IsSnapped is true.
        /// </summary>
        ISnapTarget CurrentSnapTarget { get; }
        /// <summary>
        /// Try to snap the object to a specified snap target.
        /// If the snap target is null or not available, the object will not snap.
        /// If the snap is successful, the object's position will be updated to the snap target's position,
        /// and the snap target will be notified that the object has snapped to it.
        /// </summary>
        /// <param name="snapTarget">The snap target to snap to</param>
        void TrySnapTo(ISnapTarget snapTarget);
        /// <summary>
        /// Unsnap the object from its current snap target.
        /// If the object is not snapped, this method does nothing.
        /// If the object is snapped, it will notify the current snap target that it has unsnapped,
        /// and the object's position will no longer be aligned with the snap target's position
        /// </summary>
        void UnSnap();
    }
    /// <summary>
    /// Interface for snap targets that accept snappable objects, defining snap position, availability, 
    /// and methods for handling snapping and unsnapping. Used for aligning objects in scenarios like 
    /// puzzle games or object placement, enabling flexible and reusable snap target behavior.
    /// </summary>
    public interface ISnapTarget
    {
        /// <summary>
        /// Snap position is the world-space point where a snappable object aligns when snapped to the target.
        /// Defined in the target's local space, it may be offset from the target's position.
        /// Read-only, it guides snappable objects to their snap location.
        /// </summary>
        Vector3 SnapPosition { get; }
        /// <summary>
        /// Indicates if the snap target can accept snappable objects. True if available, false if not (e.g., occupied or on cooldown).
        /// Read-only, it reflects the target's current state, guiding snappable objects' behavior with OnSnap and OnUnsnap methods.
        /// Availability may change dynamically based on game logic, enabling flexible snapping behavior.
        /// </summary>
        ISnappable CurrentSnappable { get; }
        bool IsAvailable { get; }
        /// <summary>
        /// Called when a snappable object successfully snaps to the target. 
        /// Allows the target to update state, trigger visual feedback, or notify other systems.
        /// The snapObject parameter provides access to the snapped object's properties or methods.
        /// Used with OnUnsnap for flexible snap target behavior in games or applications.
        /// </summary>
        /// <param name="snapObject">The snappable object that snapped.</param>
        void OnSnap(ISnappable snapObject);
        /// <summary>
        /// Called when a snappable object is unsnapped from the target.
        /// Allows the target to revert changes, update state, trigger visual feedback, or notify systems.
        /// The snapObject parameter provides access to the unsnapped object's properties or methods.
        /// Used with OnSnap for flexible snap target behavior, ensuring clean state for future snaps.
        /// </summary>
        /// <param name="snapObject">The snappable object that was unsnapped.</param>
        void OnUnsnap(ISnappable snapObject);
    }
}