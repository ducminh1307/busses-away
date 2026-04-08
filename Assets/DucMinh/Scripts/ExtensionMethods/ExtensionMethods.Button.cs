using System;
using UnityEngine;
using UnityEngine.UI;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void SetOnclick(this GameObject gObj, Action callback)
        {
            if (gObj == null)
            {
                Log.Debug("SetOnclick: gObj is null");
            }
            else
            {
                var button = gObj.GetOrAddComponent<Button>();
                button.onClick.AddListener(() => callback?.Invoke());
            }
        }

        public static void RemoveAllActions(this GameObject gObj)
        {
            if (gObj == null)
            {
                Log.Debug("SetOnclick: gObj is null");
            }
            else
            {
                var button = gObj.GetOrAddComponent<Button>();
                button.onClick.RemoveAllListeners();
            }
        }
    }
}