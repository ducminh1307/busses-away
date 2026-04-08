using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void SetShow(this GameObject gObject, bool show)
        {
            gObject?.SetActive(show);
        }

        public static bool IsNullObject(this GameObject gObject)
        {
            return gObject == null || !gObject;
        }

        #region Create

        public static GameObject Create(this string nameGameObject, Transform parent = null, bool worldPositionStays = true)
        {
            var gObject = new GameObject(nameGameObject);

            gObject.transform.SetParent(parent, worldPositionStays);

            return gObject;
        }

        public static GameObject Create(this string nameGameObject, Transform parent, Vector3 position, bool worldPositionStays = false)
        {
            var gObject = new GameObject(nameGameObject);

            gObject.transform.SetParent(parent, worldPositionStays);
            gObject.transform.position = position;

            return gObject;
        }

        public static GameObject Create(this GameObject prefab, Transform parent = null, bool worldPositionStays = false)
        {
            if (!prefab) return null;

            var gObject = Object.Instantiate(prefab, parent, worldPositionStays);

            return gObject;
        }

        public static GameObject Create(this GameObject prefab, Transform parent, Vector3 position, bool worldPositionStays = false)
        {
            if (!prefab) return null;

            var gObject = Object.Instantiate(prefab, parent, worldPositionStays);
            gObject.transform.position = position;

            return gObject;
        }

        public static T Create<T>(this GameObject prefab, Transform parent = null, bool worldPositionStays = false) where T : Component
        {
            if (!prefab) return null;

            var gObject = Object.Instantiate(prefab, parent, worldPositionStays);

            return gObject.GetOrAddComponent<T>();
        }

        public static T Create<T>(this GameObject prefab, Transform parent, Vector3 position, bool worldPositionStays = false) where T : Component
        {
            if (!prefab) return null;

            var gObject = Object.Instantiate(prefab, parent, worldPositionStays);
            gObject.transform.position = position;

            return gObject.GetOrAddComponent<T>();
        }

        public static T Create<T>(this string nameGameObject, Transform parent = null) where T : Component
        {
            var gObject = new GameObject(nameGameObject);

            gObject.transform.SetParent(parent);

            return gObject.GetOrAddComponent<T>();
        }

        public static T Create<T>(this string nameGameObject, Transform parent, Vector3 position, bool worldPositionStays = false) where T : Component
        {
            var gObject = new GameObject(nameGameObject);

            gObject.transform.SetParent(parent, worldPositionStays);
            gObject.transform.position = position;

            return gObject.GetOrAddComponent<T>();
        }

        public static T CreateAndGet<T>(this GameObject prefab, Transform parent = null, bool worldPositionStays = false)
        {
            if (!prefab)
            {
                Log.Warning($"CreateAndGet<{nameof(T)}>: Prefab is null");
                return default;
            }
            var gObj = Object.Instantiate(prefab, parent, worldPositionStays);
            return gObj.TryGetComponent(out T component) ? component : default;
        }

        #endregion

        public static void SetSize(this GameObject gObject, float width, float height)
        {
            if (!gObject) return;

            if (gObject.TryGetComponent(out ISetSize setSize))
            {
                setSize.SetSize(width, height);
                return;
            }

            if (gObject.TryGetComponent(out SpriteRenderer sr))
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(width, height);
            }
        }

        public static void SetOrder(this GameObject gObject, int order, string layer = "Default")
        {
            if (!gObject) return;
            if (gObject.TryGetComponent(out ISetOrder setOrder))
            {
                setOrder.SetOrder(layer, order);
            }
        }

        public static void SetSizeChildren(this GameObject gObject, float width, float height)
        {
            if (!gObject) return;
            var childCount = gObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = gObject.transform.GetChild(i);
                child.gameObject.SetSize(width, height);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gObject) where T : Component
        {
            if (gObject != null)
            {
                return gObject.TryGetComponent(out T component) ? component : gObject.AddComponent<T>();
            }

            return null;
        }

        public static T GetComponentGameObject<T>(this GameObject gObject) where T : Component
        {
            if (gObject != null)
            {
                return gObject.TryGetComponent(out T component) ? component : gObject.GetComponentInChildren<T>();
            }

            return null;
        }

        public static void SetAsFirstSibling(this GameObject gObject)
        {
            if (!gObject) return;
            gObject.transform.SetAsFirstSibling();
        }

        public static void SetAsLastSibling(this GameObject gObject)
        {
            if (!gObject) return;
            gObject.transform.SetAsLastSibling();
        }

        public static void SetSiblingOrder(this GameObject gObject, int order)
        {
            if (!gObject) return;
            gObject.transform.SetSiblingIndex(order);
        }

        public static void SetLocalPositionY(this GameObject gameObject, float positionY)
        {
            if (!gameObject) return;
            var localPosition = gameObject.transform.localPosition;
            gameObject.transform.localPosition = new Vector3(localPosition.x, positionY);
        }
    }
}