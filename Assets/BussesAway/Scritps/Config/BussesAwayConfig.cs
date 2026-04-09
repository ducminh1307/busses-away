using System;
using System.Collections.Generic;
using DucMinh;
using DucMinh.Attributes;
using UnityEngine;

namespace BussesAway
{
    public class BussesAwayConfig: BaseGameConfig<BussesAwayConfig>
    {
        [Header("Color Configs")]
        [SerializeField, ElementName(typeof(ColorType))] List<Color> colorConfigs = new();
        
        [Header("Prefab Configs")]
        
        [SerializeField] GameObject passengerPrefab;
        public static GameObject PassengerPrefab => Instance.passengerPrefab;
        
        [SerializeField] GameObject busPrefab;
        public static GameObject BusPrefab => Instance.busPrefab;
    }
}