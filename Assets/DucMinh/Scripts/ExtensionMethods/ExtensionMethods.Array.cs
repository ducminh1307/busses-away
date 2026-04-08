using System;
using System.Collections.Generic;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        public static string ConvertToString<T>(this T[] array, string separator = ",")
        {
            if (array.IsNullOrEmpty())
                return StringNull;
            
            return string.Join(separator, array);
        }

        public static T ForceGet<T>(this T[] array, int index)
        {
            if (index < 0 || index >= array.Length) return default(T);
            return array[index];
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this TValue[] array, Func<TValue, TKey> keySelector)
        {
            var dict = new Dictionary<TKey, TValue>();

            var count = array.Length;

            for (int i = 0; i < count; i++)
            {
                var value = array[i];
                var key = keySelector(value);
                if (!dict.ContainsKey(key))
                {
                    dict[key] = value;
                }
                else
                {
                    Log.Debug($"Key '{key}' already exists");
                }
            }
            
            return dict;
        }
        
        public static Dictionary<TKey1, Dictionary<TKey2, TValue>> ToDictionary<T, TKey1, TKey2, TValue>(this T[] arrays, Func<T, TKey1> key1Selector, Func<int, TKey2> key2Selector, Func<T, TValue[]> valueSelector) where TKey1 : notnull
        {
            var dict = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
        
            foreach (var element in arrays)
            {
                var key1 = key1Selector(element);
                if (!dict.ContainsKey(key1))
                {
                    var values = valueSelector(element);
                    var dict2 = new Dictionary<TKey2, TValue>();
                
                    for (int i = 0; i < values.Length; i++)
                    {
                        var sprite = values[i];
                        if (sprite != null)
                        {
                            var key2 = key2Selector(i);
                            dict2.Add(key2, sprite);
                        }
                    }
                
                    dict.Add(key1, dict2);
                }
                else
                {
                    Log.Warning($"{key1} has exist!");
                }
            }
        
            return dict;
        }
    }
}