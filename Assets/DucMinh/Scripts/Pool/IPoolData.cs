using UnityEngine;

namespace DucMinh
{
    public interface IPoolData
    {
        string PoolId { get; set; }
        GameObject PoolObject {get; set;}
        void OnReturnPool();
    }
}