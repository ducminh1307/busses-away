using UnityEngine;
using Object = UnityEngine.Object;

namespace DucMinh
{
    public class ObjectPool : BasePool<GameObject>
    {
        private Transform _parent;
        private Transform Parent
        {
            get
            {
                if (_parent.IsNullObject())
                {
                    var parentObj = (_prefab.IsNullObject()? "[Pools]": _prefab.name).Create();
                    Object.DontDestroyOnLoad(parentObj);
                    _parent = parentObj.transform;
                }
                return _parent;
            }
        }
        
        private GameObject _prefab;
        private int _prefabId = int.MinValue;
        private int PrefabId
        {
            get
            {
                if (_prefabId == int.MinValue)
                {
                    _prefabId = _prefab?.GetInstanceID() ?? -1;
                }
                return _prefabId;
            }
        }
        private string PoolId => PrefabId.ToString();

        public ObjectPool(GameObject prefab, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
        }
        
        protected override GameObject CreateInstance()
        {
            if (_prefab.IsNullObject())
            {
                Log.Error($"Prefab is null");
                return null;
            }

            var poolObj = _prefab.Create(Parent);
            var poolData = poolObj.GetOrAddComponent<PoolItemData>();
            poolData.PoolId = PoolId;
            poolData.PoolObject = poolObj;
            poolObj.SetShow(false);
            
            return poolObj;
        }

        protected override void DestroyInstance(GameObject component)
        {
            if (!component.IsNullObject()) Object.Destroy(component);
        }

        protected override bool OnRelease(GameObject component)
        {
            if (!component) return false;

            if (!component.TryGetComponent<IPoolData>(out var poolData))
            {
                Log.Debug($"{component.name} is not a pool object");
                return false;
            }
            component.transform.SetParent(Parent);
            poolData.OnReturnPool();
            return true;
        }

        protected override void OnGet(GameObject item)
        {
            if (!item) return;
            
            item.SetActive(true);
        }
    }
}