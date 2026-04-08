using UnityEngine;
using System.Collections.Generic;

namespace DucMinh
{
    [CreateAssetMenu(fileName = "DefineSymbolConfig", menuName = "DucMinh/DefineSymbolConfig")]
    public class DefineSymbolConfig : ScriptableObject
    {
        [Tooltip("List of symbols shared across the project")]
        public List<string> Symbols = new List<string>
        {
            "DEBUG_MODE",
            "SPINE_ANIMATION",
            "DOTWEEN",
            "PRIME_TWEEN",
            "REMOTE_CONFIG",
            "ADS",
            "ANALYTICS",
            "IAP_ENABLED",
            "LEVEL_PLAY_ENABLED"
        };
    }
}
