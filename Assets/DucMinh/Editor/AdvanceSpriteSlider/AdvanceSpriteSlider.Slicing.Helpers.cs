using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void ResetSliceState()
        {
            _slices.Clear();
            _sliceNames.Clear();
            _sliceBorders.Clear();
            ClearSelection();
        }

        private void ClearSelection()
        {
            _selectedSliceIndex = -1;
            _selectedSliceIndices.Clear();
        }

        private void RenameSlicesByPrefix()
        {
            _sliceNames.Clear();
            for (var i = 0; i < _slices.Count; i++)
            {
                _sliceNames.Add($"{_namingPrefix}{i}");
            }
        }

        private void EnsureSliceBordersCapacity()
        {
            while (_sliceBorders.Count < _slices.Count)
            {
                _sliceBorders.Add(_border);
            }

            while (_sliceBorders.Count > _slices.Count)
            {
                _sliceBorders.RemoveAt(_sliceBorders.Count - 1);
            }
        }

        private Vector4 GetSliceBorder(int index)
        {
            if (index < 0 || index >= _sliceBorders.Count) return _border;
            return _sliceBorders[index];
        }

        private void SetSliceBorder(int index, Vector4 border)
        {
            if (index < 0 || index >= _slices.Count) return;
            EnsureSliceBordersCapacity();
            _sliceBorders[index] = border;
        }

        private void EnsureTextureReadable()
        {
            if (_texture == null) return;

            var path = AssetDatabase.GetAssetPath(_texture);
            if (string.IsNullOrEmpty(path)) return;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.isReadable && importer.textureCompression == TextureImporterCompression.Uncompressed) return;

            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private bool IsRectEmpty(Rect rect)
        {
            if (_texture == null) return true;
            if (!TryGetClampedTextureRect(rect, out var pixelRect)) return true;

            var pixels = _texture.GetPixels(pixelRect.x, pixelRect.y, pixelRect.width, pixelRect.height);
            foreach (var color in pixels)
            {
                if (color.a > 0.01f) return false;
            }

            return true;
        }

        private bool TryGetClampedTextureRect(Rect rect, out RectInt pixelRect)
        {
            pixelRect = default;
            if (_texture == null) return false;

            var x = Mathf.Clamp(Mathf.RoundToInt(rect.x), 0, _texture.width);
            var y = Mathf.Clamp(Mathf.RoundToInt(rect.y), 0, _texture.height);
            var width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 0, _texture.width - x);
            var height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 0, _texture.height - y);
            if (width <= 0 || height <= 0) return false;

            pixelRect = new RectInt(x, y, width, height);
            return true;
        }

        private void RefreshSlicingUI()
        {
            ScheduleBuildSettingsUI();
            _previewContainer.MarkDirtyRepaint();
        }

        private ISpriteEditorDataProvider CreateSpriteDataProvider(UnityEngine.Object asset)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();

            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(asset);
            if (dataProvider != null)
            {
                dataProvider.InitSpriteEditorDataProvider();
            }

            return dataProvider;
        }

        private SpriteRect CreateSpriteRect(int index)
        {
            return new SpriteRect
            {
                name = _sliceNames[index],
                rect = _slices[index],
                alignment = _pivot,
                pivot = _customPivot,
                border = GetSliceBorder(index),
                spriteID = GUID.Generate()
            };
        }

        private string GetExportedSpriteAssetPath(string sliceName)
        {
            return _saveFolder + "/" + sliceName + ".png";
        }
    }
}
