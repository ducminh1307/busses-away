using UnityEngine;

namespace DucMinh
{
    public class InputRaycastController3D : InputControllerBase<InputHit3D>
    {
        public bool IgnoreBlockMask = true;
        public float rayDistance = 1000f;
        public LayerMask layerMask = ~0;
        public LayerMask blockMask = ~0;
        public QueryTriggerInteraction raycastTrigger = QueryTriggerInteraction.Ignore;

        protected override InputHit3D RaycastAt(Vector2 screenPos)
        {
            var cam = EnsureCamera();
            var ray = cam.ScreenPointToRay(screenPos);
            var hasHit = Physics.Raycast(ray, out var hit, rayDistance, layerMask);
            var world = hasHit ? hit.point : ray.GetPoint(10f);
            return new InputHit3D
            {
                HasHit = hasHit,
                Hit = hit,
                Position = world
            };
        }

        protected override bool ShouldBlockDueToUI(Vector2 screenPos)
        {
            if (base.ShouldBlockDueToUI(screenPos)) return true;

            if (blockMask != 0 && !IgnoreBlockMask)
            {
                var cam = EnsureCamera();
                var ray = cam.ScreenPointToRay(screenPos);
                if (Physics.Raycast(ray, out _, rayDistance, blockMask, raycastTrigger))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
