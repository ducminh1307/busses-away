using UnityEngine;

namespace DucMinh
{
    public class SingletonDestroyableBehavior<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        _instance = new GameObject(typeof(T).Name).AddComponent<T>();
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            return _instance;
                        }
#endif
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }
        
        public static bool Active => _instance != null;
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
#if UNITY_EDITOR
                // Check root game object
                var parent = transform.parent;
                if (parent != null)
                {
                    Log.Warning($"{parent.name}/{name}");
                }
#endif
                _instance = this as T;
                
                OnAwake();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnAwake()
        {

        }
        
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public static void ClearInstance()
        {
            _instance = null;
        }
    }
}