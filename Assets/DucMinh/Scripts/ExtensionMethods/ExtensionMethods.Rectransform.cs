using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        // static Vector3[] _corners = new Vector3[4];
        public static void GetAABB(this RectTransform rect, out float top, out float right, out float bottom, out float left)
        {
            // rect.GetWorldCorners(_corners);
            // left = _corners[0].x;
            // bottom = _corners[0].y;
            // right = _corners[2].x;
            // top = _corners[2].y;
            var aabb = new AABB(rect);
            top = aabb.Top;
            right = aabb.Right;
            bottom = aabb.Bottom;
            left = aabb.Left;
        }

        public static AABB GetAABB(this RectTransform rect)
        {
            var aabb = new AABB(rect);
            return aabb;
        }
    }
}