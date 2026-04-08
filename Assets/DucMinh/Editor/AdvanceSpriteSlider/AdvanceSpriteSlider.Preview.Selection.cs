using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void HandleSelectInteractions(Event e, Rect texRect)
        {
            var isAdditive = e.control || e.command;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                HandleSelectMouseDown(e, texRect, isAdditive);
                return;
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && _isSelectingArea)
            {
                HandleSelectMouseDrag(e);
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0 && _isSelectingArea)
            {
                HandleSelectMouseUp(e, texRect);
                return;
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                ClearSelectionAndRepaint();
                e.Use();
            }
        }

        private void HandleSelectMouseDown(Event e, Rect texRect, bool isAdditive)
        {
            var mousePos = e.mousePosition;
            _isSelectingArea = texRect.Contains(mousePos);

            if (!_isSelectingArea)
            {
                if (!isAdditive) ClearSelectionAndRepaint();
                return;
            }

            _selectionAdditive = isAdditive;
            _selectionMoved = false;
            _selectionStartPos = mousePos;
            _selectionScreenRect = new Rect(mousePos.x, mousePos.y, 0, 0);
            _selectionClickIndex = GetSliceIndexAtScreenPosition(mousePos, texRect);
            e.Use();
        }

        private void HandleSelectMouseDrag(Event e)
        {
            _selectionScreenRect = GetRectFromPoints(_selectionStartPos, e.mousePosition);
            if (!_selectionMoved && _selectionScreenRect.width * _selectionScreenRect.height > 9f)
            {
                _selectionMoved = true;
            }

            _previewContainer.MarkDirtyRepaint();
            e.Use();
        }

        private void HandleSelectMouseUp(Event e, Rect texRect)
        {
            if (_selectionMoved)
            {
                if (!_selectionAdditive) _selectedSliceIndices.Clear();
                SelectSlicesInScreenRect(_selectionScreenRect, texRect, additive: true);
                SyncPrimarySelection();
            }
            else
            {
                if (_selectionClickIndex >= 0)
                {
                    if (_selectionAdditive) ToggleSelection(_selectionClickIndex);
                    else SetSingleSelection(_selectionClickIndex);
                }
                else if (!_selectionAdditive)
                {
                    ClearSelectionAndRepaint();
                }
            }

            _isSelectingArea = false;
            _selectionClickIndex = -1;
            _selectionScreenRect = Rect.zero;
            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
            e.Use();
        }
    }
}
