using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void HandleEditInteractions(Event e, Rect texRect)
        {
            if (TryMoveSelectedSliceWithArrowKey(e)) return;
            if (HandleEditShortcuts(e)) return;

            HandleEditMouseHover(e, texRect);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                HandleEditMouseDown(e, texRect);
            }
            else if (e.type == EventType.MouseDrag && e.button == 0)
            {
                HandleEditMouseDrag(e, texRect);
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                HandleEditMouseUp(e);
            }
        }

        private bool HandleEditShortcuts(Event e)
        {
            if (e.type == EventType.KeyDown)
            {
                if (e.control || e.command)
                {
                    if (e.keyCode == KeyCode.C)
                    {
                        CopySelectedSlicesToClipboard();
                        e.Use();
                        return true;
                    }

                    if (e.keyCode == KeyCode.V)
                    {
                        PasteSlicesFromClipboard();
                        e.Use();
                        return true;
                    }
                }
                else if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
                {
                    DeleteSelectedSlice();
                    e.Use();
                    return true;
                }
            }

            return false;
        }

        private void HandleEditMouseHover(Event e, Rect texRect)
        {
            if (_selectedSliceIndex == -1 || _isDragging || _isResizing) return;

            var selRect = TexToScreen(_slices[_selectedSliceIndex], texRect);
            var hoverHandle = GetResizeHandleAtPosition(selRect, e.mousePosition);

            if (hoverHandle != ResizeHandle.None)
            {
                EditorGUIUtility.AddCursorRect(selRect, GetCursorForResizeHandle(hoverHandle));
            }
        }

        private void HandleEditMouseDown(Event e, Rect texRect)
        {
            var mousePos = e.mousePosition;

            if (_selectedSliceIndex != -1)
            {
                var selRect = TexToScreen(_slices[_selectedSliceIndex], texRect);
                var handle = GetResizeHandleAtPosition(selRect, mousePos);
                if (handle != ResizeHandle.None)
                {
                    StartResizingSlice(handle, mousePos, selRect);
                    e.Use();
                    return;
                }
            }

            var clickedIndex = GetSliceIndexAtScreenPosition(mousePos, texRect);

            if (clickedIndex != -1)
            {
                StartDraggingSlice(clickedIndex, mousePos, TexToScreen(_slices[clickedIndex], texRect));
            }
            else if (texRect.Contains(mousePos))
            {
                CreateAndStartResizingNewSlice(mousePos);
            }
            else
            {
                ClearSelectionAndRepaint();
            }

            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
            e.Use();
        }

        private void StartResizingSlice(ResizeHandle handle, Vector2 mousePos, Rect selRect)
        {
            BeginInteractionUndo("Resize Slice");
            _isResizing = true;
            _resizeHandle = handle;
            _dragStartPos = mousePos;
            _dragStartRect = selRect;
        }

        private void StartDraggingSlice(int index, Vector2 mousePos, Rect selRect)
        {
            BeginInteractionUndo("Move Slice");
            SetSingleSelection(index);
            _isDragging = true;
            _dragStartPos = mousePos;
            _dragStartRect = selRect;
        }

        private void CreateAndStartResizingNewSlice(Vector2 mousePos)
        {
            BeginInteractionUndo("Create Slice");
            _slices.Add(new Rect());
            _sliceNames.Add($"{_namingPrefix}{_slices.Count - 1}");
            EnsureSliceBordersCapacity();
            SetSliceBorder(_slices.Count - 1, _border);
            SetSingleSelection(_slices.Count - 1);

            _isResizing = true;
            _resizeHandle = ResizeHandle.BottomRight;
            _dragStartPos = mousePos;
            _dragStartRect = new Rect(mousePos.x, mousePos.y, 0, 0);
        }

        private void HandleEditMouseDrag(Event e, Rect texRect)
        {
            if (_selectedSliceIndex == -1) return;

            if (_isDragging)
            {
                var newScreenRect = _dragStartRect;
                newScreenRect.position += e.mousePosition - _dragStartPos;
                _slices[_selectedSliceIndex] = ScreenToTex(newScreenRect, texRect);

                _previewContainer.MarkDirtyRepaint();
                e.Use();
            }
            else if (_isResizing)
            {
                UpdateSliceResize(e.mousePosition, texRect);
                e.Use();
            }
        }

        private void UpdateSliceResize(Vector2 mousePos, Rect texRect)
        {
            var r = _dragStartRect;
            var delta = mousePos - _dragStartPos;

            switch (_resizeHandle)
            {
                case ResizeHandle.Left: r.xMin += delta.x; break;
                case ResizeHandle.Top: r.yMin += delta.y; break;
                case ResizeHandle.Right: r.xMax += delta.x; break;
                case ResizeHandle.Bottom: r.yMax += delta.y; break;
                case ResizeHandle.BottomLeft: r.xMin += delta.x; r.yMax += delta.y; break;
                case ResizeHandle.TopLeft: r.xMin += delta.x; r.yMin += delta.y; break;
                case ResizeHandle.TopRight: r.xMax += delta.x; r.yMin += delta.y; break;
                case ResizeHandle.BottomRight: r.xMax += delta.x; r.yMax += delta.y; break;
            }

            if (r.width < 0)
            {
                var rMax = r.xMax;
                r.xMax = r.xMin;
                r.xMin = rMax;
                _resizeHandle = FlipHorizontal(_resizeHandle);
                _dragStartPos.x = mousePos.x;
                _dragStartRect = r;
            }

            if (r.height < 0)
            {
                var rMax = r.yMax;
                r.yMax = r.yMin;
                r.yMin = rMax;
                _resizeHandle = FlipVertical(_resizeHandle);
                _dragStartPos.y = mousePos.y;
                _dragStartRect = r;
            }

            _slices[_selectedSliceIndex] = ScreenToTex(r, texRect);
            _previewContainer.MarkDirtyRepaint();
        }

        private void HandleEditMouseUp(Event e)
        {
            if (!_isDragging && !_isResizing) return;

            if (_selectedSliceIndex != -1)
            {
                FinalizeSliceModification();
            }

            _isDragging = false;
            _isResizing = false;
            _resizeHandle = ResizeHandle.None;

            EndInteractionUndo();
            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
            e.Use();
        }

        private void FinalizeSliceModification()
        {
            if (_slices[_selectedSliceIndex].width < 1 || _slices[_selectedSliceIndex].height < 1)
            {
                RemoveSliceAt(_selectedSliceIndex);
            }
            else
            {
                var final = _slices[_selectedSliceIndex];
                final.x = Mathf.Round(final.x);
                final.y = Mathf.Round(final.y);
                final.width = Mathf.Round(final.width);
                final.height = Mathf.Round(final.height);
                _slices[_selectedSliceIndex] = final;
            }
        }

        private void DeleteSelectedSlice()
        {
            if (_selectedSliceIndex == -1) return;

            BeginInteractionUndo("Delete Slice");
            RemoveSliceAt(_selectedSliceIndex);
            EndInteractionUndo();

            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
        }

        private bool TryMoveSelectedSliceWithArrowKey(Event e)
        {
            if (e.type != EventType.KeyDown || _texture == null || _selectedSliceIndex < 0 || _selectedSliceIndex >= _slices.Count || e.control || e.command || e.alt)
                return false;

            var step = e.shift ? 10 : 1;
            var delta = Vector2Int.zero;

            switch (e.keyCode)
            {
                case KeyCode.LeftArrow: delta.x = -step; break;
                case KeyCode.RightArrow: delta.x = step; break;
                case KeyCode.UpArrow: delta.y = step; break;
                case KeyCode.DownArrow: delta.y = -step; break;
                default: return false;
            }

            RecordUndo("Move Slice By Arrow");

            var rect = _slices[_selectedSliceIndex];
            rect.x = Mathf.Clamp(Mathf.Round(rect.x) + delta.x, 0, Mathf.Max(0, _texture.width - rect.width));
            rect.y = Mathf.Clamp(Mathf.Round(rect.y) + delta.y, 0, Mathf.Max(0, _texture.height - rect.height));
            _slices[_selectedSliceIndex] = rect;

            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
            e.Use();
            return true;
        }

        private void CopySelectedSlicesToClipboard()
        {
            _clipboardSlices.Clear();
            _clipboardNames.Clear();
            _clipboardBorders.Clear();

            var indicesToCopy = new List<int>();
            if (_selectedSliceIndices.Count > 0)
            {
                indicesToCopy.AddRange(_selectedSliceIndices);
            }
            else if (_selectedSliceIndex >= 0)
            {
                indicesToCopy.Add(_selectedSliceIndex);
            }

            if (indicesToCopy.Count == 0) return;

            foreach (var index in indicesToCopy)
            {
                if (index < 0 || index >= _slices.Count) continue;

                _clipboardSlices.Add(_slices[index]);
                var sourceName = index < _sliceNames.Count ? _sliceNames[index] : $"{_namingPrefix}{index}";
                _clipboardNames.Add(sourceName);
                _clipboardBorders.Add(index < _sliceBorders.Count ? _sliceBorders[index] : _border);
            }
        }

        private void PasteSlicesFromClipboard()
        {
            if (_clipboardSlices.Count == 0) return;

            BeginInteractionUndo("Paste Slice");
            _selectedSliceIndices.Clear();

            const float offsetStep = 8f;
            for (var i = 0; i < _clipboardSlices.Count; i++)
            {
                var rect = _clipboardSlices[i];
                rect.x = Mathf.Clamp(rect.x + offsetStep, 0, Mathf.Max(0, _texture.width - rect.width));
                rect.y = Mathf.Clamp(rect.y + offsetStep, 0, Mathf.Max(0, _texture.height - rect.height));

                _slices.Add(rect);

                var sourceName = i < _clipboardNames.Count ? _clipboardNames[i] : $"{_namingPrefix}{_slices.Count - 1}";
                _sliceNames.Add(GetUniqueSliceName($"{sourceName}_copy"));
                _sliceBorders.Add(i < _clipboardBorders.Count ? _clipboardBorders[i] : _border);

                _selectedSliceIndices.Add(_slices.Count - 1);
            }

            SyncPrimarySelection();
            EndInteractionUndo();
            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
        }
    }
}
