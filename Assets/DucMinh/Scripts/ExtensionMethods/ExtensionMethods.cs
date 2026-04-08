using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static readonly string StringNull = "null";
        public static readonly string StringListEmpty = "empty";
        
        public static string GetString(this object obj)
        {
            return obj != null? obj.ToString() : StringNull;
        }

        public static string ToBoldString(this object obj)
        {
            return obj != null? $"<b>{obj}</b>" : StringNull;
        }

        public static string ToColorString(this object obj, Color color)
        {
            var colorHex = color.ToHex();
            return obj != null? $"<color=#{colorHex}>{obj}</color>" : StringNull;
        }
        
        public static string ToColorString(this object obj, string colorHex)
        {
            var isColor = colorHex.IsHexColor();
            return obj != null? (isColor? $"<color=#{colorHex}>{obj}</color>" : obj.ToString()): StringNull;
        }
    }
}
