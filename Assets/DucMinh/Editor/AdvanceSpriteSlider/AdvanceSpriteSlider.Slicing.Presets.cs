using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void SavePreset()
        {
            var path = EditorUtility.SaveFilePanel("Save Slicer Preset", "Assets", "SlicerPreset", "json");
            if (string.IsNullOrEmpty(path)) return;

            var preset = new SlicerPreset
            {
                cellSize = _cellSize,
                padding = _padding,
                offset = _offset,
                cellCount = _cellCount,
                sliceMode = (int)_sliceMode
            };

            System.IO.File.WriteAllText(path, JsonUtility.ToJson(preset, true));
            AssetDatabase.Refresh();
        }

        private void LoadPreset()
        {
            var path = EditorUtility.OpenFilePanel("Load Slicer Preset", "Assets", "json");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var preset = JsonUtility.FromJson<SlicerPreset>(System.IO.File.ReadAllText(path));
                Undo.RecordObject(this, "Load Preset");
                _cellSize = preset.cellSize;
                _padding = preset.padding;
                _offset = preset.offset;
                _cellCount = preset.cellCount;
                _sliceMode = (SliceMode)preset.sliceMode;
                RefreshSlicingUI();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load preset: " + e.Message);
            }
        }

        private void OnScrollWheel(WheelEvent evt)
        {
            if (_texture == null) return;

            var mousePos = evt.localMousePosition;
            var layoutRect = _previewContainer.layout;
            var viewCenter = layoutRect.center;
            var mouseToCenter = mousePos - viewCenter - _scrollPos;

            var oldZoom = _zoom;
            var zoomDelta = evt.delta.y * -0.05f;
            _zoom = Mathf.Clamp(_zoom + zoomDelta, 0.1f, 10f);

            if (oldZoom != _zoom)
            {
                var zoomRatio = _zoom / oldZoom;
                _scrollPos -= mouseToCenter * (zoomRatio - 1f);
            }

            evt.StopPropagation();
            _previewContainer.MarkDirtyRepaint();
        }
    }
}
