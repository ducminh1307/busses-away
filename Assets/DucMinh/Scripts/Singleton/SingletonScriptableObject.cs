using UnityEngine;

namespace DucMinh
{
    public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        protected static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                
                _instance = Resources.Load<T>(typeof(T).Name);
                if (_instance != null) return _instance;
                
                _instance = Resources.Load<T>(typeof(T).FullName);
                if (_instance != null) return _instance;

                Helper.CreateScriptableObjectWithNamespace(_instance);
                return _instance;
            }
        }
    }
}