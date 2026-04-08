using System;
using System.Collections.Generic;
using System.Drawing;
using Color = UnityEngine.Color;

namespace DucMinh
{
    public static partial class UIManager
    {
        private static readonly Stack<UIType> _UIQueue = new();
        private static Dictionary<UIType, BaseUI> _UIActives = new();
        
        public static void ShowOrCreateUI<T>(this UIType uiType, Action<T> onShow = null) where T : BaseUI
        {
            var uiConfig = UIConfig.GetUIConfigItem(uiType);
            if (uiConfig == null)
            {
                Log.Warning($"UIConfigItem not found for UIType: {uiType.ToBoldString()}");
                return;
            }
            
            if (_UIActives.TryGetValue(uiType, out var uiObjectActive) && !uiObjectActive.IsNullObject())
            {
                Log.Warning("UIType is already active: " + uiType.ToBoldString());
                return;
            }
            
            var canvas = uiConfig.CanvasType == CanvasType.Static ? StaticCanvas : DynamicCanvas;
            var uiObject = Get(uiType, canvas);

            if (uiObject.IsNullObject()) return;
            
            var uiComponent = uiObject.GetComponent<T>();
            if (uiComponent.IsNullObject())
            {
                Log.Debug("UI Component of type " + typeof(T).ToBoldString().ToColorString(Color.red) + " not found on UIType: " + uiType.ToBoldString().ToColorString(Color.red));
                return;
            }
            
            uiObject.SetAsLastSibling();
            canvas.SetAsLastSibling();
            
            _UIActives[uiType] = uiComponent;
            _UIQueue.Push(uiType);
            
            onShow?.Invoke(uiComponent);
            uiComponent.Show();
        }

        public static void HideUI(this UIType uiType, Action onHide = null)
        {
            if (!_UIActives.Remove(uiType, out var uiActive))
            {
                Log.Warning("UIType is not active: " + uiType);
                return;
            }

            if (_UIQueue.Count > 0 && _UIQueue.Peek() == uiType)
            {
                _UIQueue.Pop();
            }
            else
            {
                Log.Warning("UIType is not on top of the UI stack: " + uiType);
            }
            
            uiActive.Hide(() =>
            {
                Release(uiType, uiActive.gameObject);
                onHide?.Invoke();
            });
        }
        
        public static UIType GetTopUI()
        {
            if (_UIQueue.Count == 0)
            {
                Log.Warning("No active UI to get.");
                return UIType.None;
            }

            var topUIType = _UIQueue.Peek();
            return topUIType;
        }
        
        public static void GetUI<T>(this UIType uiType, Action<T> onGet) where T : BaseUI
        {
            if (_UIActives.TryGetValue(uiType, out var uiActive) && !uiActive.IsNullObject())
            {
                var uiComponent = uiActive.GetComponent<T>();
                if (uiComponent.IsNullObject())
                {
                    Log.Debug($"UI Component of type {typeof(T).ToBoldString()}  not found on UIType: {uiType.ToBoldString()}");
                    return;
                }
                
                onGet?.Invoke(uiComponent);
            }
            else
            {
                Log.Warning($"UIType is not active: {uiType.ToBoldString()}");
            }
        }

        public static void Clear()
        {
            _UIQueue.Clear();
            _UIActives.Clear();
            ClearPool();
        }
    }
}