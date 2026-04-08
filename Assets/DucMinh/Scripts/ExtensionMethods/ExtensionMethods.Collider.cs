using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void AddLayerExclude(this Collider collider, LayerMask layerMask)
        {
            collider.excludeLayers |= 1 << layerMask;
        }

        public static void AddLayerExclude(this Collider2D collider, string layerName)
        {
            var layerMask = LayerMask.NameToLayer(layerName);
            collider.excludeLayers |= layerMask;
        }

        public static void RemoveLayerExclude(this Collider collider, LayerMask layerMask)
        {
            collider.excludeLayers &= ~(1 << layerMask);
        }

        public static void RemoveLayerExclude(this Collider2D collider, string layerName)
        {
            var layerMask = LayerMask.NameToLayer(layerName);
            collider.excludeLayers &= ~(1 << layerMask);
        }
    }
}