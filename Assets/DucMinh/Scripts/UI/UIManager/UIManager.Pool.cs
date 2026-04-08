using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public static partial class UIManager
    {
        private static Dictionary<UIType, ObjectPool> _UIObjectPools = new();

        private static GameObject Get(UIType uiType, Transform canvas)
        {
            if (!_UIObjectPools.TryGetValue(uiType, out var pool))
            {
                var uiPrefab = UIConfig.GetUIPrefab(uiType);
                if (uiPrefab.IsNullObject())
                {
                    Log.Warning("UIPrefab not found for UIType: " + uiType);
                    return null;
                }
                pool = new ObjectPool(uiPrefab, canvas);
                _UIObjectPools[uiType] = pool;
            }
            var uiObject = pool.Get();
            return uiObject;
        }

        private static void Release(UIType uiType, GameObject uiObject)
        {
            if (!_UIObjectPools.TryGetValue(uiType, out var pool))
            {
                Log.Warning("UIObjectPool not found for UIType: " + uiType);
                return;
            }
            pool.Release(uiObject);
        }

        private static void ClearPool()
        {
            foreach (var pool in _UIObjectPools.Values)
            {
                pool.Clear();
            }
            _UIObjectPools.Clear();
        }
    }
}