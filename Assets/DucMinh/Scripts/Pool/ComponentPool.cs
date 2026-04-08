using UnityEngine;
using Object = UnityEngine.Object;

namespace DucMinh
{
    public class ComponentPool<T> : BasePool<T> where T : Component
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

        public ComponentPool(GameObject prefab, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
        }
        
        protected override T CreateInstance()
        {
            if (_prefab.IsNullObject())
            {
                Log.Error($"Prefab is null");
                return null;
            }

            var component = _prefab.CreateAndGet<T>(Parent);
            var poolData = component.GetOrAddComponent<PoolItemData>();
            poolData.PoolId = PoolId;
            poolData.PoolObject = component.gameObject;
            component.SetShow(false);
            
            return component;
        }

        protected override void DestroyInstance(T component)
        {
            if (!component.IsNullObject()) Object.Destroy(component.gameObject);
        }

        protected override bool OnRelease(T component)
        {
            if (!component) return false;

            if (!component.TryGetComponent<IPoolData>(out var poolData))
            {
                Log.Debug($"{component.name} is not a pool object");
                return false;
            }
            component.SetParent(Parent);
            poolData.OnReturnPool();
            return true;
        }

        protected override void OnGet(T item)
        {
            if (!item) return;
            
            item.SetShow(true);
        }
    }
}