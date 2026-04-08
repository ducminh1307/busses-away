using System;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static float GetTotalSeconds(this DateTime? dateTime, DateTime other)
        {
            if (dateTime.HasValue)
            {
                return (float) dateTime.Value.Subtract(other).TotalSeconds;
            }

            return 0;
        }
    }
}