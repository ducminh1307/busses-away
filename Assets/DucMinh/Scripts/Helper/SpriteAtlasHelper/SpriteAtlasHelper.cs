using System;
using UnityEngine;
using UnityEngine.U2D;

namespace DucMinh
{
    public static class SpriteAtlasHelper
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Registers a callback to handle asynchronous loading of SpriteAtlases (Late Binding).
        /// This is required if your Atlases have 'Include in Build' unchecked and are loaded via Addressables/AssetBundles.
        /// Call this once in your GameManager.Init() or at app startup.
        /// </summary>
        public static void InitializeLateBinding()
        {
            if (_isInitialized) return;
            SpriteAtlasManager.atlasRequested += OnAtlasRequested;
            _isInitialized = true;
            Log.Debug("[SpriteAtlasHelper] Late Binding initialized. Listening for missing atlases...");
        }

        private static void OnAtlasRequested(string atlasTag, Action<SpriteAtlas> callback)
        {
            Log.Warning($"Atlas late binding requested for tag: {atlasTag}.");
            
            /*
            // 💡 UNCOMMENT AND USE THIS IF YOUR FRAMEWORK USES ADDRESSABLES:
            // Assuming your atlas address matches the atlasTag exactly.
            
            AddressableHelper.LoadAssetAsync<SpriteAtlas>(atlasTag, (atlas) => 
            {
                if (atlas != null)
                {
                    callback(atlas);
                    Log.Debug($"Successfully loaded SpriteAtlas '{atlasTag}' via Addressables.");
                }
                else
                {
                    Log.Error($"Failed to load SpriteAtlas '{atlasTag}'.");
                }
            });
            */
        }

        /// <summary>
        /// Retrieves a sprite from the Atlas and safely removes the "(Clone)" suffix that Unity appends at runtime.
        /// Useful if you rely on strict string matching for UI/Logic.
        /// </summary>
        public static Sprite GetSpriteSafe(this SpriteAtlas atlas, string spriteName)
        {
            if (atlas == null)
            {
                Log.Error($"Cannot GetSprite '{spriteName}'. The provided SpriteAtlas is null.");
                return null;
            }

            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            Sprite sprite = atlas.GetSprite(spriteName);
            
            // Unity dynamically instances sprites from an Atlas at runtime.
            // It appends "(Clone)" to their names which can break Name evaluations.
            if (sprite != null && sprite.name.EndsWith("(Clone)"))
            {
                sprite.name = sprite.name.Replace("(Clone)", "");
            }
            
            if (sprite == null)
            {
                Log.Warning($"Sprite '{spriteName}' not found in Atlas '{atlas.name}'.");
            }
            
            return sprite;
        }
    }
}
