using UnityEditor;
using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void OnPreviewGUI()
        {
            if (_texture == null) return;

            var e = Event.current;
            var layoutRect = _previewContainer.layout;

            DrawCheckerBoard(layoutRect);
            HandlePreviewScrolling(e);

            var viewCenter = layoutRect.center;
            var minZoom = Mathf.Min(layoutRect.width / _texture.width, layoutRect.height / _texture.height) * 0.1f;
            _zoom = Mathf.Max(_zoom, minZoom);

            var texSize = new Vector2(_texture.width * _zoom, _texture.height * _zoom);
            var drawPos = viewCenter + _scrollPos - texSize / 2f;
            var texRect = new Rect(drawPos.x, drawPos.y, texSize.x, texSize.y);

            GUI.DrawTexture(texRect, _texture, ScaleMode.StretchToFill);

            HandleInteractions(e, texRect);
            DrawSlices(texRect);
            DrawSelectionArea();
        }

        private void HandlePreviewScrolling(Event e)
        {
            if (e.type == EventType.MouseDrag && (e.button == 2 || e.button == 1))
            {
                _scrollPos += e.delta;
                _previewContainer.MarkDirtyRepaint();
            }
        }

        private void DrawSelectionArea()
        {
            if (_interactionMode == InteractionMode.Select && _isSelectingArea)
            {
                var c = new Color(0.2f, 0.8f, 1f, 0.18f);
                EditorGUI.DrawRect(_selectionScreenRect, c);
                Handles.color = new Color(0.2f, 0.8f, 1f, 1f);
                Handles.DrawWireCube(_selectionScreenRect.center, _selectionScreenRect.size);
            }
        }

        private void InitCheckerTexture()
        {
            if (_checkerTex != null) return;

            _checkerTex = new Texture2D(2, 2)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            var c1 = new Color(0.15f, 0.15f, 0.15f, 1f);
            var c2 = new Color(0.2f, 0.2f, 0.2f, 1f);
            _checkerTex.SetPixels(new[] { c1, c2, c2, c1 });
            _checkerTex.Apply();
        }

        private void DrawCheckerBoard(Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;

            InitCheckerTexture();
            const float size = 40f;
            GUI.DrawTextureWithTexCoords(rect, _checkerTex, new Rect(0, 0, rect.width / size, rect.height / size));
        }

        private Rect TexToScreen(Rect r, Rect texRect)
        {
            var y = _texture.height - r.yMax;
            return new Rect(
                texRect.x + r.x * _zoom,
                texRect.y + y * _zoom,
                r.width * _zoom,
                r.height * _zoom
            );
        }

        private Rect ScreenToTex(Rect sr, Rect texRect)
        {
            var w = sr.width / _zoom;
            var h = sr.height / _zoom;
            var x = (sr.x - texRect.x) / _zoom;
            var yTop = (sr.y - texRect.y) / _zoom;
            var y = _texture.height - yTop - h;

            return new Rect(x, y, w, h);
        }

        private void DrawSlices(Rect texRect)
        {
            if (Event.current.type != EventType.Repaint) return;

            var viewRect = new Rect(0, 0, _previewContainer.layout.width, _previewContainer.layout.height);

            for (var i = 0; i < _slices.Count; i++)
            {
                var screenRect = TexToScreen(_slices[i], texRect);
                if (!screenRect.Overlaps(viewRect)) continue;

                var isSelected = _selectedSliceIndices.Contains(i);
                DrawSingleSlicePreview(i, screenRect, isSelected);

                if (isSelected && _interactionMode == InteractionMode.Edit && i == _selectedSliceIndex)
                {
                    DrawSliceEditHandles(screenRect);
                    DrawSliceEditLabels(i, screenRect);
                }
            }
        }

        private void DrawSingleSlicePreview(int index, Rect screenRect, bool isSelected)
        {
            Handles.color = isSelected ? Color.green : Color.yellow;
            Handles.DrawWireCube(screenRect.center, screenRect.size);
            DrawSliceBorderPreview(screenRect, _slices[index], GetSliceBorder(index), isSelected);
        }

        private void DrawSliceEditHandles(Rect screenRect)
        {
            DrawHandle(new Vector2(screenRect.xMin, screenRect.yMax));
            DrawHandle(new Vector2(screenRect.xMin, screenRect.yMin));
            DrawHandle(new Vector2(screenRect.xMax, screenRect.yMin));
            DrawHandle(new Vector2(screenRect.xMax, screenRect.yMax));
        }

        private void DrawSliceEditLabels(int index, Rect screenRect)
        {
            EnsureSliceNamesCapacity();
            var nameRect = new Rect(screenRect.x, screenRect.y - 40, 200, 20);
            var labelRect = new Rect(screenRect.x, screenRect.y - 20, 200, 20);

            GUI.backgroundColor = new Color(255, 255, 255, 0.3f);
            GUI.Label(nameRect, _sliceNames[index], GUI.skin.box);
            GUI.Label(labelRect, $"[{_slices[index].width}x{_slices[index].height}]", GUI.skin.box);
            GUI.backgroundColor = Color.white;
        }

        private void DrawSliceBorderPreview(Rect screenRect, Rect sliceRect, Vector4 border, bool isSelected)
        {
            if (screenRect.width <= 0f || screenRect.height <= 0f) return;
            if (sliceRect.width <= 0f || sliceRect.height <= 0f) return;

            var left = Mathf.Clamp(border.x, 0f, sliceRect.width);
            var right = Mathf.Clamp(border.z, 0f, sliceRect.width);
            var bottom = Mathf.Clamp(border.y, 0f, sliceRect.height);
            var top = Mathf.Clamp(border.w, 0f, sliceRect.height);

            if (left <= 0f && right <= 0f && bottom <= 0f && top <= 0f) return;

            var leftX = Mathf.Clamp(screenRect.xMin + left * _zoom, screenRect.xMin, screenRect.xMax);
            var rightX = Mathf.Clamp(screenRect.xMax - right * _zoom, screenRect.xMin, screenRect.xMax);
            var topY = Mathf.Clamp(screenRect.yMin + top * _zoom, screenRect.yMin, screenRect.yMax);
            var bottomY = Mathf.Clamp(screenRect.yMax - bottom * _zoom, screenRect.yMin, screenRect.yMax);

            Handles.color = isSelected ? new Color(0.2f, 1f, 1f, 0.95f) : new Color(0.2f, 1f, 1f, 0.55f);

            if (left > 0f) Handles.DrawLine(new Vector3(leftX, screenRect.yMin), new Vector3(leftX, screenRect.yMax));
            if (right > 0f) Handles.DrawLine(new Vector3(rightX, screenRect.yMin), new Vector3(rightX, screenRect.yMax));
            if (top > 0f) Handles.DrawLine(new Vector3(screenRect.xMin, topY), new Vector3(screenRect.xMax, topY));
            if (bottom > 0f) Handles.DrawLine(new Vector3(screenRect.xMin, bottomY), new Vector3(screenRect.xMax, bottomY));
        }

        private void DrawHandle(Vector2 pos)
        {
            var handleRect = new Rect(pos.x - HANDLE_SIZE / 2, pos.y - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE);
            EditorGUI.DrawRect(handleRect, Color.green);
            Handles.color = Color.black;
            Handles.DrawWireCube(handleRect.center, handleRect.size);
        }

        private ResizeHandle GetResizeHandleAtPosition(Rect rect, Vector2 mousePos)
        {
            var cornerRadius = HANDLE_SIZE * 1.5f;
            var edgeRange = HANDLE_SIZE;

            var bottomLeft = new Vector2(rect.xMin, rect.yMax);
            var topLeft = new Vector2(rect.xMin, rect.yMin);
            var topRight = new Vector2(rect.xMax, rect.yMin);
            var bottomRight = new Vector2(rect.xMax, rect.yMax);

            if (Vector2.Distance(mousePos, bottomLeft) <= cornerRadius) return ResizeHandle.BottomLeft;
            if (Vector2.Distance(mousePos, topLeft) <= cornerRadius) return ResizeHandle.TopLeft;
            if (Vector2.Distance(mousePos, topRight) <= cornerRadius) return ResizeHandle.TopRight;
            if (Vector2.Distance(mousePos, bottomRight) <= cornerRadius) return ResizeHandle.BottomRight;

            var insideVerticalRange = mousePos.y >= rect.yMin - edgeRange && mousePos.y <= rect.yMax + edgeRange;
            var insideHorizontalRange = mousePos.x >= rect.xMin - edgeRange && mousePos.x <= rect.xMax + edgeRange;

            if (insideVerticalRange && Mathf.Abs(mousePos.x - rect.xMin) <= edgeRange) return ResizeHandle.Left;
            if (insideVerticalRange && Mathf.Abs(mousePos.x - rect.xMax) <= edgeRange) return ResizeHandle.Right;
            if (insideHorizontalRange && Mathf.Abs(mousePos.y - rect.yMin) <= edgeRange) return ResizeHandle.Top;
            if (insideHorizontalRange && Mathf.Abs(mousePos.y - rect.yMax) <= edgeRange) return ResizeHandle.Bottom;

            return ResizeHandle.None;
        }

        private static MouseCursor GetCursorForResizeHandle(ResizeHandle handle)
        {
            return handle switch
            {
                ResizeHandle.Left or ResizeHandle.Right => MouseCursor.ResizeHorizontal,
                ResizeHandle.Top or ResizeHandle.Bottom => MouseCursor.ResizeVertical,
                ResizeHandle.TopLeft or ResizeHandle.BottomRight => MouseCursor.ResizeUpLeft,
                ResizeHandle.TopRight or ResizeHandle.BottomLeft => MouseCursor.ResizeUpRight,
                _ => MouseCursor.Arrow,
            };
        }

        private static ResizeHandle FlipHorizontal(ResizeHandle handle)
        {
            return handle switch
            {
                ResizeHandle.Left => ResizeHandle.Right,
                ResizeHandle.Right => ResizeHandle.Left,
                ResizeHandle.TopLeft => ResizeHandle.TopRight,
                ResizeHandle.TopRight => ResizeHandle.TopLeft,
                ResizeHandle.BottomLeft => ResizeHandle.BottomRight,
                ResizeHandle.BottomRight => ResizeHandle.BottomLeft,
                _ => handle,
            };
        }

        private static ResizeHandle FlipVertical(ResizeHandle handle)
        {
            return handle switch
            {
                ResizeHandle.Top => ResizeHandle.Bottom,
                ResizeHandle.Bottom => ResizeHandle.Top,
                ResizeHandle.TopLeft => ResizeHandle.BottomLeft,
                ResizeHandle.BottomLeft => ResizeHandle.TopLeft,
                ResizeHandle.TopRight => ResizeHandle.BottomRight,
                ResizeHandle.BottomRight => ResizeHandle.TopRight,
                _ => handle,
            };
        }

        private void HandleInteractions(Event e, Rect texRect)
        {
            if (_texture == null) return;

            if (_interactionMode == InteractionMode.Select)
            {
                HandleSelectInteractions(e, texRect);
            }
            else
            {
                HandleEditInteractions(e, texRect);
            }
        }
    }
}
