using System;
using System.Collections.Generic;

namespace DucMinh
{
    public static partial class Helper
    {
        public static int GetEnumCount<T>() where T : struct
        {
            return Enum.GetNames(typeof(T)).Length;
        }

        public static int GetEnumCount<T>(this string enumName) where T : struct
        {
            return (int)(object)Enum.Parse<T>(enumName);
        }

        public static List<T> GetListEnum<T>() where T : struct
        {
            var count = GetEnumCount<T>();
            var list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                list.Add((T)Enum.Parse(typeof(T), Enum.GetName(typeof(T), i) ?? string.Empty));
            }
            return list;
        }
    }
}