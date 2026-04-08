using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public class GameObjectReference : MonoBehaviour
    {
        
        [SerializeField] private List<GameObjectReferenceData> references = new();
        [SerializeField] private List<GameObjectReferenceData> poolReferences = new();
        
        public List<GameObjectReferenceData> References => references;
        public List<GameObjectReferenceData> PoolReferences => poolReferences;
    }
}