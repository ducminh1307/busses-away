using UnityEngine;

namespace DucMinh
{
    public class BaseGameConfig<T>: SingletonScriptableObject<T> where T : BaseGameConfig<T>
    {
        #region Camera

        [Header("Camera configs")]
        [SerializeField] private float minCameraSize = 4f;
        public static float MinCameraSize => Instance.minCameraSize;

        #endregion

        #region Input

        [Header("Input configs")]
        [SerializeField] private LayerMask checkInputLayer;
        public static LayerMask CheckInputLayer => Instance.checkInputLayer;
        
        [SerializeField] private LayerMask blockInputLayer;
        public static LayerMask BlockInputLayer => Instance.blockInputLayer;

        #endregion  
    }
}