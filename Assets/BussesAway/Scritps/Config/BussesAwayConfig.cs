using System;
using System.Collections.Generic;
using DucMinh;
using UnityEngine;

namespace BussesAway
{
    [Serializable]
    public class ColorData: ThreeProperties
    {
        public ColorType colorType;
        public Material characterMaterial;
        public Material busMaterial;
    }
    
    public class BussesAwayConfig: BaseGameConfig<BussesAwayConfig>
    {
        [SerializeField] List<ColorData> colorConfigs = new List<ColorData>();
    }
}