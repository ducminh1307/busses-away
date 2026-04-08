using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void SetSortingLayer(this SpriteRenderer spriteRenderer, string sortingLayerName)
        {
            if (spriteRenderer == null)
            {
                Log.Debug("SpriteRenderer is null");
                return;
            }

            if (SortingLayer.NameToID(sortingLayerName) != 0)
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
            }
            else
            {
                Log.Debug($"Sorting layer '{sortingLayerName}' does not exist.");
            }
        }
        
        public static void SetSortingLayer(this SpriteRenderer spriteRenderer, int sortingLayerID)
        {
            if (spriteRenderer == null)
            {
                Log.Debug("SpriteRenderer is null");
                return;
            }

            if (SortingLayer.IsValid(sortingLayerID))
            {
                spriteRenderer.sortingLayerID = sortingLayerID;
            }
            else
            {
                Log.Debug($"Invalid sorting layer ID:'{sortingLayerID}'");
            }
        }

        public static void SetSortingOrder(this SpriteRenderer spriteRenderer, int sortingOrder)
        {
            if (spriteRenderer == null)
            {
                Log.Debug("SpriteRenderer is null");
                return;
            }
            
            spriteRenderer.sortingOrder = sortingOrder;
        }

        public static string GetSortingLayerName(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer?.sortingLayerName ?? StringNull;
        }

        public static int GetSortingLayerID(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer?.sortingLayerID ?? 0;
        }

        public static int GetSortingOrder(this SpriteRenderer spriteRenderer)
        {
            return spriteRenderer?.sortingOrder ?? 0;
        }
    }
}