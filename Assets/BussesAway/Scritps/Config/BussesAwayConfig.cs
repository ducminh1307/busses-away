using System;
using System.Collections.Generic;
using DucMinh;
using DucMinh.Attributes;
using UnityEngine;

namespace BussesAway
{
    [Serializable]
    public class ColorData: ThreeProperties
    {
        public Color characterColor;
        public Material busMaterial;
    }
    
    public class BussesAwayConfig: BaseGameConfig<BussesAwayConfig>
    {
        [SerializeField, ElementName(typeof(ColorType))] List<Color> colorConfigs = new();
    }
}