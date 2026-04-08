using UnityEngine;

namespace DucMinh
{
    public class InputRaycastController2D : InputControllerBase<InputHit2D>
    {
        public LayerMask layerMask = ~0;
        public bool includeTriggers = true;

        protected override InputHit2D RaycastAt(Vector2 screenPos)
        {
            var cam = EnsureCamera();
            var world = cam.ScreenToWorldPoint(new Vector2(screenPos.x, screenPos.y));
            Vector2 wp = world;
            var hit = Physics2D.Raycast(wp, Vector2.zero, 0f, layerMask);
            return new InputHit2D
            {
                HasHit = hit.collider != null && (includeTriggers || !hit.collider.isTrigger),
                Hit = hit,
                WorldPosition = wp
            };
        }
    }
}
