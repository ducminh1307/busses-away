using System;
using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static T TryGetComponent<T>(this GameObject gObj)
        {
            if (gObj == null) return default;
            
            var component = gObj.GetComponent<T>();
            return component;
        }
        
        public static T GetOrAddComponent<T>(this Component cpn) where T : Component
        {
            if (cpn == null) return null;
            if (!cpn.gameObject.TryGetComponent(out T component))
            {
                component = cpn.gameObject.AddComponent<T>();
            }
            return component;
        }

        public static bool IsNullObject(this Component component)
        {
            return component == null || !component;
        }
        
        public static void SetOrder(this Component component, int order, string layer = "Default")
        {
            if (!component) return;
            if (component.TryGetComponent(out ISetOrder setOrder))
            {
                setOrder.SetOrder(layer, order);
            }
        }

        public static void ForEachChildren(this Component component, Action<Transform, int> callback = null)
        {
            if (component == null || !component) return;
            var transform = component.transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                callback?.Invoke(transform.GetChild(i), i);
            }
        }
        
        public static void ForEachChildren(this Component component, Action<Transform> callback = null)
        {
            if (component == null || !component) return;
            var transform = component.transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                callback?.Invoke(transform.GetChild(i));
            }
        }
        
        public static void ForEachChildren<T>(this Component component, Action<T, int> callback = null) where T : Component
        {
            if (component == null || !component) return;
            var transform = component.transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var cpn = child.GetComponent<T>();
                callback?.Invoke(cpn, i);
            }
        }
        
        public static void ForEachChildren<T>(this Component component, Action<T> callback = null) where T : Component
        {
            if (component == null || !component) return;
            var transform = component.transform;
            var childCount = transform.childCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var cpn = child.GetComponent<T>();
                callback?.Invoke(cpn);
            }
        }
    }
}