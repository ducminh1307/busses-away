using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DucMinh
{
#if UNITY_EDITOR
    public static class GizmosHelper
    {
        public static void DrawCircle(Vector3 center, float radius, int edges = 32, Color? color = null)
        {
            if (edges <= 8)
            {
                Log.Warning("Edges should be greater or equal than 8");
                edges = 8;
            }
            
            var oldColor = Gizmos.color;
            if (color.HasValue) Gizmos.color = color.Value;
            
            float angle = 360f / edges;
            
            Vector3 prePoint = center + new Vector3(Mathf.Cos(0), Mathf.Sin(0)) * radius;

            for (int i = 1; i <= edges; i++)
            {
                float rad = angle * i * Mathf.Deg2Rad;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
                Gizmos.DrawLine(prePoint, nextPoint);
                prePoint = nextPoint;
            }
            
            Gizmos.color = oldColor;
        }

        public static void DrawSquare2D(Vector2 center, Vector2 size, Color? color = null)
        {
            var oldColor = Gizmos.color;
            if (color.HasValue) Gizmos.color = color.Value;
            
            if (size.x <= 0 || size.y <= 0) return;
            
            Vector3 half = size / 2f;
            
            Vector2 topLeft = center + new Vector2(-half.x, half.y);
            Vector2 topRight = center + new Vector2(half.x, half.y);
            Vector2 bottomRight = center + new Vector2(half.x, -half.y);
            Vector2 bottomLeft = center + new Vector2(-half.x, -half.y);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
            
            Gizmos.color = oldColor;
        }

        public static void DrawGrid(Vector2 topLeft, int rows, int columns, float cellWidth, float cellHeight,
            float spacing = 0f, Color? boardColor = null, Color? cellColor = null)
        {
            var oldColor = Gizmos.color;

            var boardWidth  = cellWidth  * columns + spacing * (columns - 1);
            var boardHeight = cellHeight * rows    + spacing * (rows    - 1);
            var center = new Vector2(topLeft.x + boardWidth * 0.5f, topLeft.y - boardHeight * 0.5f);

            // Draw board outline
            DrawSquare2D(center, new Vector2(boardWidth, boardHeight), boardColor);

            // Draw each cell
            if (cellColor.HasValue) Gizmos.color = cellColor.Value;
            else Gizmos.color = oldColor;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < columns; col++)
                {
                    var posX = topLeft.x + (col + 0.5f) * cellWidth  + col * spacing;
                    var posY = topLeft.y - (row + 0.5f) * cellHeight - row * spacing;
                    DrawSquare2D(new Vector2(posX, posY), new Vector2(cellWidth, cellHeight));
                }
            }

            Gizmos.color = oldColor;
        }

        public static void DrawFillAABB(Vector2 center, Vector2 size, int numberLine = 50, Color? color = null)
        {
            Color oldColor = Gizmos.color;
            if (color.HasValue) Gizmos.color = color.Value;

            Vector2 half = size * 0.5f;
            float left = center.x - half.x;
            float right = center.x + half.x;
            float bottom = center.y - half.y;
            float top = center.y + half.y;

            // Vẽ khung viền AABB
            Vector2 topLeft     = new Vector2(left, top);
            Vector2 topRight    = new Vector2(right, top);
            Vector2 bottomRight = new Vector2(right, bottom);
            Vector2 bottomLeft  = new Vector2(left, bottom);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            float spacing = size.x / (numberLine + 1);
            for (int i = 1; i <= numberLine; i++)
            {
                float x = left + spacing * i;
                Vector3 from = new Vector3(x, bottom, 0);
                Vector3 to = new Vector3(x, top, 0);
                Gizmos.DrawLine(from, to);
            }

            Gizmos.color = oldColor;
        }
        
        public static void DrawFillAABB(Vector2 center, float size, int numberLine = 50, Color? color = null)
        {
            var sizeVector2 = new Vector2(size, size);
            
            DrawFillAABB(center, sizeVector2, numberLine, color);
        }
        
        public static void DrawArrow(Vector3 from, Vector3 to, float headLength = 0.2f, float headAngle = 20f, Color? color = null)
        {
            Color oldColor = Gizmos.color;
            if (color.HasValue) Gizmos.color = color.Value;

            Gizmos.DrawLine(from, to);

            Vector3 direction = (to - from).normalized;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + headAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - headAngle, 0) * Vector3.forward;

            Gizmos.DrawLine(to, to + right * headLength);
            Gizmos.DrawLine(to, to + left * headLength);

            Gizmos.color = oldColor;
        }
        
        public static void DrawText(Vector3 position, string text, Color? color = null, int size = 12)
        {
            Color oldColor = Gizmos.color;
            // Lưu lại GUI style cũ
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                fontSize = size,
                fontStyle = FontStyle.Bold
            };

            if (color.HasValue)
            {
                style.normal.textColor = color.Value;
            }
            Handles.Label(position, text, style);
            Gizmos.color = oldColor;
        }
    }
#endif
}