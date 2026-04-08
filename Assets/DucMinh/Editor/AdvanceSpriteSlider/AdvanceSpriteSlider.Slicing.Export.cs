using UnityEditor;
using UnityEngine;

namespace DucMinh.AdvanceSpriteSlider
{
    public partial class AdvanceSpriteSlider
    {
        private void ApplyToTexture()
        {
            if (_texture == null) return;

            var texturePath = AssetDatabase.GetAssetPath(_texture);
            if (string.IsNullOrEmpty(texturePath)) return;

            EnsureSliceNamesCapacity();
            EnsureSliceBordersCapacity();
            EnsureTextureReadable();

            var fullFolderPath = GetFullSaveFolderPath();
            EnsureSaveFolderExists(fullFolderPath);
            ExportSlicesToPngFiles(fullFolderPath);

            AssetDatabase.Refresh();
            ApplyExportedSpriteImportSettings();
            ApplySliceMetadataToSourceTexture(texturePath);

            Debug.Log($"Exported {_slices.Count} sprites to {_saveFolder}");
        }

        private string GetFullSaveFolderPath()
        {
            var projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
            return System.IO.Path.IsPathRooted(_saveFolder)
                ? _saveFolder
                : System.IO.Path.GetFullPath(System.IO.Path.Combine(projectPath, _saveFolder));
        }

        private static void EnsureSaveFolderExists(string folderPath)
        {
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
        }

        private void ExportSlicesToPngFiles(string fullFolderPath)
        {
            for (var i = 0; i < _slices.Count; i++)
            {
                if (!TryGetClampedTextureRect(_slices[i], out var pixelRect)) continue;

                var filePath = System.IO.Path.Combine(fullFolderPath, _sliceNames[i] + ".png");
                ExportSliceToPng(pixelRect, filePath);
            }
        }

        private void ExportSliceToPng(RectInt pixelRect, string filePath)
        {
            var pixels = _texture.GetPixels(pixelRect.x, pixelRect.y, pixelRect.width, pixelRect.height);
            var exportedTexture = new Texture2D(pixelRect.width, pixelRect.height, TextureFormat.RGBA32, false);

            try
            {
                exportedTexture.SetPixels(pixels);
                exportedTexture.Apply();
                var pngData = exportedTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(filePath, pngData);
            }
            finally
            {
                DestroyImmediate(exportedTexture);
            }
        }

        private void ApplyExportedSpriteImportSettings()
        {
            for (var i = 0; i < _sliceNames.Count; i++)
            {
                var textureImporter = AssetImporter.GetAtPath(GetExportedSpriteAssetPath(_sliceNames[i])) as TextureImporter;
                if (textureImporter == null) continue;

                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;

                var dataProvider = CreateSpriteDataProvider(textureImporter);
                if (dataProvider != null)
                {
                    var spriteRects = dataProvider.GetSpriteRects();
                    if (spriteRects != null && spriteRects.Length > 0)
                    {
                        spriteRects[0].alignment = _pivot;
                        spriteRects[0].pivot = _customPivot;
                        spriteRects[0].border = GetSliceBorder(i);
                        dataProvider.SetSpriteRects(spriteRects);
                        dataProvider.Apply();
                    }
                }

                EditorUtility.SetDirty(textureImporter);
                textureImporter.SaveAndReimport();
            }
        }

        private void ApplySliceMetadataToSourceTexture(string texturePath)
        {
            var sourceImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (sourceImporter == null) return;

            sourceImporter.spriteImportMode = _slices.Count > 1 ? SpriteImportMode.Multiple : SpriteImportMode.Single;

            var dataProvider = CreateSpriteDataProvider(sourceImporter);
            if (dataProvider != null)
            {
                var spriteRects = new SpriteRect[_slices.Count];
                for (var i = 0; i < _slices.Count; i++)
                {
                    spriteRects[i] = CreateSpriteRect(i);
                }

                dataProvider.SetSpriteRects(spriteRects);
                dataProvider.Apply();
            }

            EditorUtility.SetDirty(sourceImporter);
            sourceImporter.SaveAndReimport();
        }

        private void LoadSaveFolderPreference()
        {
            var savedPath = EditorPrefs.GetString(SaveFolderPrefsKey, "Assets");
            _saveFolder = AssetDatabase.IsValidFolder(savedPath) ? savedPath : "Assets";
        }

        private bool TrySetSaveFolder(UnityEngine.Object folderObject)
        {
            if (folderObject == null)
            {
                _saveFolder = "Assets";
                EditorPrefs.SetString(SaveFolderPrefsKey, _saveFolder);
                return true;
            }

            var folderPath = AssetDatabase.GetAssetPath(folderObject);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning("Save Folder must be a folder asset.");
                return false;
            }

            _saveFolder = folderPath;
            EditorPrefs.SetString(SaveFolderPrefsKey, _saveFolder);
            return true;
        }

        private void LoadExistingSlices()
        {
            Undo.RecordObject(this, "Revert Slices");
            ResetSliceState();

            if (_texture == null) return;

            var path = AssetDatabase.GetAssetPath(_texture);
            if (string.IsNullOrEmpty(path)) return;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple) return;

            var dataProvider = CreateSpriteDataProvider(importer);
            if (dataProvider == null) return;

            var spriteRects = dataProvider.GetSpriteRects();
            foreach (var spriteRect in spriteRects)
            {
                _slices.Add(spriteRect.rect);
                _sliceNames.Add(spriteRect.name);
                _sliceBorders.Add(spriteRect.border);
            }

            if (spriteRects.Length <= 0) return;

            _pivot = spriteRects[0].alignment;
            _customPivot = spriteRects[0].pivot;
            _border = spriteRects[0].border;
        }
    }
}
