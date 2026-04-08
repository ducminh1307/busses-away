using UnityEngine;

namespace DucMinh
{
    public class InputController : InputControllerBase<InputHit2D>
    {
        public LayerMask overlapMask = ~0;
        public bool useOverlapPoint = false;

        protected override InputHit2D RaycastAt(Vector2 screenPos)
        {
            var cam = EnsureCamera();
            var wp3 = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            Vector2 wp = wp3;

            if (!useOverlapPoint)
                return new InputHit2D { HasHit = false, WorldPosition = wp };

            var hitCol = Physics2D.OverlapPoint(wp, overlapMask);
            return new InputHit2D
            {
                HasHit = hitCol != null,
                Hit = new RaycastHit2D(),
                WorldPosition = wp
            };
        }
    }
}