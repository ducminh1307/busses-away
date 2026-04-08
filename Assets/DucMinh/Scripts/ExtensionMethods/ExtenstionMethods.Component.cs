using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void SetShow(this Component component,  bool isActive)
        {
            component.gameObject.SetActive(isActive);
        }

        public static void SetParent(this Component component, Transform parent)
        {
            component.transform.parent = parent;
        }
        
        public static void SetParent(this Component component, Transform parent, Vector3 position)
        {
            component.transform.parent = parent;
            component.transform.position = position;
        }
    }
}