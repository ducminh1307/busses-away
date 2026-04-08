using UnityEngine;

namespace DucMinh
{
    public struct AABB
    {
        private float _top;
        private float _right;
        private float _bottom;
        private float _left;

        // ── Properties ─────────────────────────────────────────────────

        public float Top => _top;
        public float Right => _right;
        public float Bottom => _bottom;
        public float Left => _left;

        public float Width => _right - _left;
        public float Height => _top - _bottom;

        public Vector2 Min => new(_left, _bottom);
        public Vector2 Max => new(_right, _top);
        public Vector2 Center => new((_left + _right) * 0.5f, (_top + _bottom) * 0.5f);
        public Vector2 Size => new(Width, Height);
        public Vector2 Extents => Size * 0.5f;

        public Vector2 TopLeft => new(_left, _top);
        public Vector2 TopRight => new(_right, _top);
        public Vector2 BottomLeft => new(_left, _bottom);
        public Vector2 BottomRight => new(_right, _bottom);

        // ── Constructors ───────────────────────────────────────────────

        /// <summary>Tạo AABB từ 4 cạnh trực tiếp.</summary>
        public AABB(float left, float bottom, float right, float top)
        {
            _left = left;
            _bottom = bottom;
            _right = right;
            _top = top;
        }

        /// <summary>Tạo AABB từ <see cref="Rect"/>.</summary>
        public AABB(Rect rect)
        {
            _left = rect.xMin;
            _bottom = rect.yMin;
            _right = rect.xMax;
            _top = rect.yMax;
        }

        /// <summary>Tạo AABB từ <see cref="Bounds"/> (bỏ trục Z).</summary>
        public AABB(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            _left = min.x;
            _bottom = min.y;
            _right = max.x;
            _top = max.y;
        }

        /// <summary>Tạo AABB từ <see cref="RectTransform"/> (world-space corners).</summary>
        public AABB(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners); // [0] BL, [1] TL, [2] TR, [3] BR

            Vector3 bl = corners[0];
            Vector3 tr = corners[2];

            _left = bl.x;
            _bottom = bl.y;
            _right = tr.x;
            _top = tr.y;
        }

        // ── Static Factory Methods ─────────────────────────────────────

        /// <summary>Tạo AABB từ 2 góc min (bottom-left) và max (top-right).</summary>
        public static AABB FromMinMax(Vector2 min, Vector2 max)
        {
            return new AABB(min.x, min.y, max.x, max.y);
        }

        /// <summary>Tạo AABB từ 2 điểm bất kỳ (tự sắp xếp lại min/max).</summary>
        public static AABB FromTwoPoints(Vector2 a, Vector2 b)
        {
            return new AABB(
                Mathf.Min(a.x, b.x),
                Mathf.Min(a.y, b.y),
                Mathf.Max(a.x, b.x),
                Mathf.Max(a.y, b.y)
            );
        }

        /// <summary>Tạo AABB từ center và size.</summary>
        public static AABB FromCenterSize(Vector2 center, Vector2 size)
        {
            var half = size * 0.5f;
            return new AABB(
                center.x - half.x,
                center.y - half.y,
                center.x + half.x,
                center.y + half.y
            );
        }

        /// <summary>Tạo AABB từ center và extents (nửa size).</summary>
        public static AABB FromCenterExtents(Vector2 center, Vector2 extents)
        {
            return new AABB(
                center.x - extents.x,
                center.y - extents.y,
                center.x + extents.x,
                center.y + extents.y
            );
        }

        /// <summary>Tạo AABB từ <see cref="Collider2D"/> (world-space bounds).</summary>
        public static AABB FromCollider2D(Collider2D collider)
        {
            return new AABB(collider.bounds);
        }

        /// <summary>Tạo AABB từ <see cref="SpriteRenderer"/> (world-space bounds).</summary>
        public static AABB FromSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            return new AABB(spriteRenderer.bounds);
        }

        /// <summary>Tạo AABB từ <see cref="Renderer"/> bất kỳ.</summary>
        public static AABB FromRenderer(Renderer renderer)
        {
            return new AABB(renderer.bounds);
        }

        /// <summary>Tạo AABB bao quanh vùng nhìn thấy của Camera ortho (world-space).</summary>
        public static AABB FromCamera(Camera camera)
        {
            float halfH = camera.orthographicSize;
            float halfW = halfH * camera.aspect;
            Vector2 center = camera.transform.position;
            return new AABB(
                center.x - halfW,
                center.y - halfH,
                center.x + halfW,
                center.y + halfH
            );
        }

        /// <summary>Tạo AABB nhỏ nhất bao quanh tập hợp các điểm.</summary>
        public static AABB FromPoints(params Vector2[] points)
        {
            if (points == null || points.Length == 0)
                return default;

            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            return new AABB(minX, minY, maxX, maxY);
        }

        /// <summary>Tạo AABB là phần hợp (union) của 2 AABB.</summary>
        public static AABB Union(AABB a, AABB b)
        {
            return new AABB(
                Mathf.Min(a._left, b._left),
                Mathf.Min(a._bottom, b._bottom),
                Mathf.Max(a._right, b._right),
                Mathf.Max(a._top, b._top)
            );
        }

        /// <summary>Tạo AABB là phần giao (intersection) của 2 AABB. Trả về default nếu không giao nhau.</summary>
        public static AABB Intersection(AABB a, AABB b)
        {
            float left = Mathf.Max(a._left, b._left);
            float bottom = Mathf.Max(a._bottom, b._bottom);
            float right = Mathf.Min(a._right, b._right);
            float top = Mathf.Min(a._top, b._top);

            if (left > right || bottom > top)
                return default;

            return new AABB(left, bottom, right, top);
        }

        // ── Query Methods ──────────────────────────────────────────────

        /// <summary>Kiểm tra 2 AABB có giao nhau không.</summary>
        public bool Intersects(AABB other)
        {
            return !(other._left > _right || other._right < _left ||
                     other._top < _bottom || other._bottom > _top);
        }

        /// <summary>Kiểm tra điểm có nằm bên trong AABB không.</summary>
        public bool Contains(Vector2 point)
        {
            return point.x >= _left && point.x <= _right &&
                   point.y >= _bottom && point.y <= _top;
        }

        /// <summary>Kiểm tra AABB khác có nằm hoàn toàn bên trong không.</summary>
        public bool Contains(AABB other)
        {
            return other._left >= _left && other._right <= _right &&
                   other._bottom >= _bottom && other._top <= _top;
        }

        /// <summary>Tính khoảng cách ngắn nhất từ điểm đến AABB (0 nếu nằm bên trong).</summary>
        public float DistanceTo(Vector2 point)
        {
            float dx = Mathf.Max(_left - point.x, 0, point.x - _right);
            float dy = Mathf.Max(_bottom - point.y, 0, point.y - _top);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Tìm điểm gần nhất trên AABB so với điểm cho trước.</summary>
        public Vector2 ClosestPoint(Vector2 point)
        {
            return new Vector2(
                Mathf.Clamp(point.x, _left, _right),
                Mathf.Clamp(point.y, _bottom, _top)
            );
        }

        // ── Manipulation Methods ───────────────────────────────────────

        /// <summary>Mở rộng AABB ra mỗi hướng một khoảng amount.</summary>
        public AABB Expand(float amount)
        {
            return new AABB(_left - amount, _bottom - amount, _right + amount, _top + amount);
        }

        /// <summary>Mở rộng AABB theo từng trục (x, y).</summary>
        public AABB Expand(Vector2 amount)
        {
            return new AABB(
                _left - amount.x, _bottom - amount.y,
                _right + amount.x, _top + amount.y
            );
        }

        /// <summary>Mở rộng AABB để bao gồm thêm một điểm.</summary>
        public AABB Encapsulate(Vector2 point)
        {
            return new AABB(
                Mathf.Min(_left, point.x),
                Mathf.Min(_bottom, point.y),
                Mathf.Max(_right, point.x),
                Mathf.Max(_top, point.y)
            );
        }

        /// <summary>Mở rộng AABB để bao gồm thêm một AABB khác.</summary>
        public AABB Encapsulate(AABB other)
        {
            return Union(this, other);
        }

        // ── Conversion ─────────────────────────────────────────────────

        /// <summary>Chuyển thành <see cref="Rect"/>.</summary>
        public Rect ToRect()
        {
            return new Rect(_left, _bottom, Width, Height);
        }

        /// <summary>Chuyển thành <see cref="Bounds"/> (z = 0).</summary>
        public Bounds ToBounds()
        {
            return new Bounds((Vector3)Center, new Vector3(Width, Height, 0f));
        }

        // ── Operators ──────────────────────────────────────────────────

        public static bool operator ==(AABB a, AABB b)
        {
            return Mathf.Approximately(a._left, b._left) &&
                   Mathf.Approximately(a._bottom, b._bottom) &&
                   Mathf.Approximately(a._right, b._right) &&
                   Mathf.Approximately(a._top, b._top);
        }

        public static bool operator !=(AABB a, AABB b) => !(a == b);

        public override bool Equals(object obj) => obj is AABB other && this == other;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _left.GetHashCode();
                hash = hash * 31 + _bottom.GetHashCode();
                hash = hash * 31 + _right.GetHashCode();
                hash = hash * 31 + _top.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"AABB(L={_left:F2}, B={_bottom:F2}, R={_right:F2}, T={_top:F2} | {Width:F2}x{Height:F2})";
        }
    }
}