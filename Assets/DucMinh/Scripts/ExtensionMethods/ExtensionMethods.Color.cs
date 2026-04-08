using System.Text.RegularExpressions;
using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static string ToHex(this Color color)
        {
            var r = (byte) Mathf.Clamp(Mathf.RoundToInt(color.r * byte.MaxValue), 0, byte.MaxValue);
            var g = (byte) Mathf.Clamp(Mathf.RoundToInt(color.g * byte.MaxValue), 0, byte.MaxValue);
            var b = (byte) Mathf.Clamp(Mathf.RoundToInt(color.b * byte.MaxValue), 0, byte.MaxValue);
            var a = (byte) Mathf.Clamp(Mathf.RoundToInt(color.a * byte.MaxValue), 0, byte.MaxValue);

            return a == 255 ? $"{r:X2}{g:X2}{b:X2}" : $"{r:X2}{g:X2}{b:X2}{a:X2}";
        }
        
        public static bool IsHexColor(this string hex)
        {
            // Biểu thức chính quy để kiểm tra các định dạng HEX hợp lệ
            var pattern = "^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$";
            return Regex.IsMatch(hex, pattern);
        }

        public static Color ToColor(this string hex)
        {
            if (!hex.IsHexColor())
                return Color.white;

            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
        }
    }
}