using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void ClearSelectionAndRepaint()
        {
            _selectedSliceIndices.Clear();
            _selectedSliceIndex = -1;
            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
        }

        private void RemoveSliceAt(int index)
        {
            _slices.RemoveAt(index);
            _sliceNames.RemoveAt(index);
            if (index < _sliceBorders.Count)
            {
                _sliceBorders.RemoveAt(index);
            }

            RebuildSelectionAfterSliceRemoval(index);
            _selectedSliceIndex = -1;
        }

        private string GetUniqueSliceName(string baseName)
        {
            baseName = string.IsNullOrWhiteSpace(baseName) ? _namingPrefix : baseName;
            var candidate = baseName;
            var suffix = 1;

            while (_sliceNames.Contains(candidate))
            {
                candidate = $"{baseName}_{suffix}";
                suffix++;
            }

            return candidate;
        }

        private int GetSliceIndexAtScreenPosition(Vector2 screenPos, Rect texRect)
        {
            for (var i = _slices.Count - 1; i >= 0; i--)
            {
                if (TexToScreen(_slices[i], texRect).Contains(screenPos)) return i;
            }

            return -1;
        }

        private static Rect GetRectFromPoints(Vector2 a, Vector2 b)
        {
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        private void SetSingleSelection(int index)
        {
            _selectedSliceIndices.Clear();
            if (index >= 0 && index < _slices.Count)
            {
                _selectedSliceIndices.Add(index);
                _selectedSliceIndex = index;
            }
            else
            {
                _selectedSliceIndex = -1;
            }
        }

        private void ToggleSelection(int index)
        {
            if (index < 0 || index >= _slices.Count) return;

            if (_selectedSliceIndices.Contains(index))
            {
                _selectedSliceIndices.Remove(index);
            }
            else
            {
                _selectedSliceIndices.Add(index);
            }

            SyncPrimarySelection();
        }

        private void SelectSlicesInScreenRect(Rect selectionRect, Rect texRect, bool additive)
        {
            if (!additive) _selectedSliceIndices.Clear();

            for (var i = 0; i < _slices.Count; i++)
            {
                var screenRect = TexToScreen(_slices[i], texRect);
                if (screenRect.Overlaps(selectionRect) && !_selectedSliceIndices.Contains(i))
                {
                    _selectedSliceIndices.Add(i);
                }
            }
        }

        private void SyncPrimarySelection()
        {
            if (_selectedSliceIndices.Count == 0)
            {
                _selectedSliceIndex = -1;
                return;
            }

            if (!_selectedSliceIndices.Contains(_selectedSliceIndex))
            {
                _selectedSliceIndex = _selectedSliceIndices[_selectedSliceIndices.Count - 1];
            }
        }

        private void RebuildSelectionAfterSliceRemoval(int removedIndex)
        {
            for (var i = _selectedSliceIndices.Count - 1; i >= 0; i--)
            {
                var selected = _selectedSliceIndices[i];
                if (selected == removedIndex)
                {
                    _selectedSliceIndices.RemoveAt(i);
                }
                else if (selected > removedIndex)
                {
                    _selectedSliceIndices[i] = selected - 1;
                }
            }
        }
    }
}
