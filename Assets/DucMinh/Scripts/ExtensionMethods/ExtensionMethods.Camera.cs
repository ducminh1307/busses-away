using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static Vector3 GetTopLeftPosition(this Camera camera)
        {
            var pos = camera.transform.position;
            return new Vector3(pos.x - camera.orthographicSize * camera.aspect, pos.y + camera.orthographicSize);
        }

        public static Vector3 GetBottomRightPosition(this Camera camera)
        {
            var pos = camera.transform.position;
            return new Vector3(pos.x + camera.orthographicSize * camera.aspect, pos.y - camera.orthographicSize);
        }

        public static void GetSize(this Camera camera, out float width, out float height)
        {
            height = camera.orthographicSize * 2;
            width = height * camera.aspect;
        }

        public static Vector3 GetCenter(this Camera camera)
        {
            return camera.transform.position;
        }

        public static float GetWidth(this Camera camera)
        {
            return camera.orthographicSize * 2f * camera.aspect;
        }

        public static float GetHeight(this Camera camera)
        {
            return camera.orthographicSize * 2f;
        }

        public static Bounds GetBounds(this Camera camera)
        {
            var center = camera.GetCenter();
            var size = new Vector3(camera.GetWidth(), camera.GetHeight(), 0f);
            return new Bounds(center, size);
        }

        public static Vector3 GetTopRightPosition(this Camera camera)
        {
            var pos = camera.transform.position;
            return new Vector3(pos.x + camera.orthographicSize * camera.aspect, pos.y + camera.orthographicSize);
        }

        public static Vector3 GetBottomLeftPosition(this Camera camera)
        {
            var pos = camera.transform.position;
            return new Vector3(pos.x - camera.orthographicSize * camera.aspect, pos.y - camera.orthographicSize);
        }

        public static Vector3[] GetWorldCorners(this Camera camera)
        {
            return new[]
            {
                camera.GetTopLeftPosition(),
                camera.GetTopRightPosition(),
                camera.GetBottomRightPosition(),
                camera.GetBottomLeftPosition()
            };
        }

        public static bool IsVisible(this Camera camera, Vector3 worldPosition)
        {
            var viewportPoint = camera.WorldToViewportPoint(worldPosition);
            return viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                   viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
                   viewportPoint.z > 0f;
        }

        public static bool IsVisible(this Camera camera, Bounds bounds)
        {
            var cameraBounds = camera.GetBounds();
            return cameraBounds.Intersects(bounds);
        }

        public static Vector3 ViewportToWorldPoint(this Camera camera, Vector2 viewportPosition)
        {
            var worldPoint = camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, camera.nearClipPlane));
            worldPoint.z = 0f;
            return worldPoint;
        }

        public static Vector3 GetRandomPositionInView(this Camera camera)
        {
            var randomViewport = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            return camera.ViewportToWorldPoint(randomViewport);
        }

        public static Vector3 ClampToBounds(this Camera camera, Vector3 worldPosition)
        {
            var pos = camera.transform.position;
            var halfWidth = camera.orthographicSize * camera.aspect;
            var halfHeight = camera.orthographicSize;

            return new Vector3(
                Mathf.Clamp(worldPosition.x, pos.x - halfWidth, pos.x + halfWidth),
                Mathf.Clamp(worldPosition.y, pos.y - halfHeight, pos.y + halfHeight),
                worldPosition.z
            );
        }
    }
}