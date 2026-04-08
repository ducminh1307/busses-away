#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace DucMinh.Editor
{
    public static class SpriteAtlasEditorHelper
    {
        [MenuItem("Assets/DucMinh/Sprite Atlas/Create Atlas From Folder", false, 10)]
        public static void CreateAtlasFromFolder()
        {
            var selectedObj = Selection.activeObject;
            if (selectedObj == null) return;

            string folderPath = AssetDatabase.GetAssetPath(selectedObj);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning("Please select a valid folder to create a SpriteAtlas.");
                return;
            }

            string folderName = Path.GetFileName(folderPath);
            string atlasPath = $"{folderPath}/{folderName}.spriteatlas";

            if (File.Exists(atlasPath))
            {
                Debug.LogWarning($"Atlas already exists at {atlasPath}");
                return;
            }

            SpriteAtlas atlas = new SpriteAtlas();
            
            // Standard recommended settings for 2D UI and Gameplay
            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false, // Rotation can break Canvas UI, false is safer
                enableTightPacking = false, // Tight packing can cause bleeding in UI
                padding = 4
            };
            atlas.SetPackingSettings(packingSettings);

            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            atlas.SetTextureSettings(textureSettings);

            // Save the asset to disk FIRST before modifying extensions
            AssetDatabase.CreateAsset(atlas, atlasPath);

            // Add the folder as the target object for this atlas
            SpriteAtlasExtensions.Add(atlas, new Object[] { selectedObj });

            // Pack immediately so it's ready to use
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00ff00><b>[Sprite Atlas]</b> Successfully created and packed Atlas at {atlasPath}</color>");
            Selection.activeObject = atlas;
            EditorGUIUtility.PingObject(atlas);
        }

        // Validate method to ensure the option only appears when right-clicking a folder
        [MenuItem("Assets/DucMinh/Sprite Atlas/Create Atlas From Folder", true)]
        public static bool ValidateCreateAtlasFromFolder()
        {
            return Selection.activeObject != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        // [MenuItem("Tools/DucMinh/Sprite Atlas/Pack All Atlases in Project")]
        public static void PackAllAtlases()
        {
            string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas");
            if (guids.Length == 0)
            {
                Debug.LogWarning("No SpriteAtlases found in the project.");
                return;
            }

            SpriteAtlas[] atlases = new SpriteAtlas[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                atlases[i] = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            }

            SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"<color=#00ff00><b>[Sprite Atlas]</b> Packed {atlases.Length} SpriteAtlases successfully!</color>");
        }
    }
}
#endif
