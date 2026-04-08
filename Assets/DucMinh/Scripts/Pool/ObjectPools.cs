using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public static class ObjectPools
    {
        private static readonly Dictionary<string, ObjectPool> _prefabPools = new();
        
        private static Transform _poolParent;
        private static Transform PoolParent
        {
            get
            {
                if (_poolParent == null)
                {
                    var parentObj = new GameObject("[ObjectPools_Root]");
                    Object.DontDestroyOnLoad(parentObj);
                    _poolParent = parentObj.transform;
                }
                return _poolParent;
            }
        }

        private static ObjectPool GetPool(GameObject prefab)
        {
            if (prefab == null)
            {
                Log.Error("Prefab must be not null!");
                return null;
            }

            var instanceID = prefab.GetInstanceID().ToString();
            if (!_prefabPools.TryGetValue(instanceID, out var pool))
            {
                pool = new ObjectPool(prefab, PoolParent);
                _prefabPools[instanceID] = pool;
            }
            return pool;
        }
        
        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var pool = GetPool(prefab);
            if (pool == null) return null;

            var obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            
            return obj;
        }
        
        public static GameObject Get(GameObject prefab)
        {
             var pool = GetPool(prefab);
             return pool?.Get();
        }
        
        public static void Release(GameObject obj)
        {
            if (obj == null) return;

            if (obj.TryGetComponent<IPoolData>(out var poolData))
            {
                var instanceID = poolData.PoolId;
                if (_prefabPools.TryGetValue(instanceID, out var pool))
                {
                    pool.Release(obj);
                }
            }
            else
            {
                Log.Warning($"Object {obj.name} haven't component IPoolData!");
            }
        }
        
        public static void ClearScenePools()
        {
            // TODO: Quyết định pool nào là "scene pool" và pool nào là "global pool"
            foreach (var pool in _prefabPools.Values)
            {
                pool.Clear();
            }
            _prefabPools.Clear();
        }
    }
}