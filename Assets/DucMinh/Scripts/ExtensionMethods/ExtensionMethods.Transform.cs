using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void Clear(this Transform transform)
        {
            if (transform == null) return;

            if (Application.isPlaying)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        public static T GetComponentGameObject<T>(this Transform transform) where T : Component
        {
            T component = transform.GetComponent<T>();
            
            if (component == null)
                component = component.gameObject.GetComponentInChildren<T>();
            
            if (component == null)
                component =  transform.GetComponentInParent<T>();

            return component;
        }

        public static void SetPosition<T>(this T component, Vector3 position) where T : Component
        {
            if (component == null) return;
            
            component.transform.position = position;
        }
        
        public static void SetLocalPosition<T>(this T component, Vector3 position) where T : Component
        {
            if (component == null) return;
            
            component.transform.localPosition = position;
        }

        public static void SetScale<T>(this T component, float scale) where T : Component
        {
            if (component == null) return;
            
            component.transform.localScale = new Vector3(scale, scale, scale);
        }
        
        public static void SetScale<T>(this T component, Vector3 scale) where T : Component
        {
            if (component == null) return;
            
            component.transform.localScale = scale;
        }
        
        public static void DestroyChildren(this Transform transform)
        {
            if (transform == null) return;
            if (Application.isPlaying)
            {
                for (int i = transform.childCount - 1; i >= 0 ; i--)
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
            }
            else
            {
                for (int i = transform.childCount - 1; i >= 0 ; i--)
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
    }
}