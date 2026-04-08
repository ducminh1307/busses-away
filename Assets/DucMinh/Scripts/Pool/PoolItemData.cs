using UnityEngine;

namespace DucMinh
{
    public class PoolItemData : MonoBehaviour, IPoolData
    {
        private GameObject poolObject;
        private string poolId;
        public string PoolId
        {
            get => poolId;
            set => poolId = value;
        }

        public GameObject PoolObject
        {
            get => poolObject;
            set => poolObject = value;
        }

        public void OnReturnPool()
        {
            gameObject.SetShow(false);
        }
    }
}