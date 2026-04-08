// Assets/DucMinh/JigsawMesh/EarClipTriangulator.cs
using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public static class EarClipTriangulator
    {
        /// <summary>
        /// Triangulate đa giác đơn (simple polygon) theo thuật toán Ear Clipping.
        /// Trả về danh sách chỉ số tam giác (3 chỉ số/tam giác).
        /// Yêu cầu đa giác không tự cắt (self-intersecting).
        /// Đa giác nên theo chiều kim đồng hồ (CW).
        /// Nếu CCW thì kết quả vẫn đúng nhưng mặt lật ngược (backface).
        /// Thời gian O(n^2), n = số đỉnh.
        /// Tham khảo: https://en.wikipedia.org/wiki/Polygon_triangulation#Ear_clipping_method
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static List<int> Triangulate(List<Vector2> poly)
        {
            var indices = new List<int>();
            int n = poly.Count;
            if (n < 3) return indices;

            var V = new List<int>(n);
            for (int i = 0; i < n; i++) V.Add(i);

            int countSafe = 2 * n; // chống kẹt
            while (V.Count > 2 && countSafe-- > 0)
            {
                bool earFound = false;
                for (int i = 0; i < V.Count; i++)
                {
                    int i0 = V[(i + V.Count - 1) % V.Count];
                    int i1 = V[i];
                    int i2 = V[(i + 1) % V.Count];

                    var a = poly[i0];
                    var b = poly[i1];
                    var c = poly[i2];

                    if (!IsConvex(a, b, c)) continue;
                    if (ContainsPoint(poly, V, i0, i1, i2)) continue;

                    indices.Add(i0);
                    indices.Add(i1);
                    indices.Add(i2);
                    V.RemoveAt(i);
                    earFound = true;
                    break;
                }
                if (!earFound) break; // đa giác tự cắt? → dừng
            }
            return indices;
        }

        /// <summary>
        /// Diện tích có dấu (signed area) của đa giác.
        /// >0 nếu CCW, <0 nếu CW.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
        {
            return Cross(b - a, c - b) < 0f; // CW polygon
        }

        /// <summary>
        /// Cross product 2D (bỏ z).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        static bool PointInTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            var v0 = c - a; var v1 = b - a; var v2 = p - a;
            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);
            float invDen = 1f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDen;
            float v = (dot00 * dot12 - dot01 * dot02) * invDen;
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        static bool ContainsPoint(List<Vector2> poly, List<int> V, int i0, int i1, int i2)
        {
            var a = poly[i0]; var b = poly[i1]; var c = poly[i2];
            for (int k = 0; k < V.Count; k++)
            {
                int vi = V[k];
                if (vi == i0 || vi == i1 || vi == i2) continue;
                if (PointInTri(poly[vi], a, b, c)) return true;
            }
            return false;
        }
    }
}
