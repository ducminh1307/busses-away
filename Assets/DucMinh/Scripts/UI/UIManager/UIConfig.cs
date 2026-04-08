using System;
using System.Collections.Generic;
using System.Linq;
using DucMinh.Attributes;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

namespace DucMinh
{
    public enum UIType
    {
        None,
        MainMenu,
        MainGame,
        Setting,
    }

    public enum CanvasType
    {
        Static,
        Dynamic,
    }
    
    [Serializable]
    public class UIConfigItem : ThreeProperties
    {
        public UIType UIType;
        public CanvasType CanvasType;
        public GameObject Prefab;
    }
    
    public class UIConfig : SingletonScriptableObject<UIConfig>
    {
        [SerializeField, ElementName("UI Item")]
        private List<UIConfigItem> _uiConfigItems = new();
        
        private Dictionary<UIType, UIConfigItem> _uiConfigDict = new();

        private Dictionary<UIType, UIConfigItem> _UiConfigDict
        {
            get
            {
                if (_uiConfigDict.IsNullOrEmpty())
                {
                    _uiConfigDict = _uiConfigItems.ToDictionary(item => item.UIType, item => item);
                }
                return _uiConfigDict;
            }
        }

        public static UIConfigItem GetUIConfigItem(UIType uiType)
        {
            return Instance._UiConfigDict.GetValueOrDefault(uiType);
        }
        
        public static GameObject GetUIPrefab(UIType uiType)
        {
            return Instance._UiConfigDict.TryGetValue(uiType, out var uiConfigItem) ? uiConfigItem.Prefab : null;
        }
    }
}