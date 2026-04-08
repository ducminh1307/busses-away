using System;
using System.Collections.Generic;
using System.Linq;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }
        public static bool GetBool(this Dictionary<string, object> dictionary, string key, bool defaultValue = false)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }
            
            try
            {
                var result = value.ToString();
                if (result.Length == 1)
                    return result[0] == '1';

                return bool.TryParse(result, out var b) ? b : defaultValue;
            }
            catch (Exception e)
            {
                Log.Warning($"Can't convert value of key '{key}' to int. Exception: {e}");
                return defaultValue;
            }
        }
        
        public static int GetInt(this Dictionary<string, object> dictionary, string key, int defaultValue = 0)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }
            
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception e)
            {
                Log.Warning($"Can't convert value of key '{key}' to int. Exception: {e}");
                return defaultValue;
            }
        }

        public static string GetString(this Dictionary<string, object> dictionary, string key, string defaultValue = "")
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }

            try
            {
                return value.ToString();
            }
            catch (Exception e)
            {
                Log.Warning($"Can't convert value of key '{key}' to string. Exception: {e}");
                return defaultValue;
            }
        }

        public static float GetFloat(this Dictionary<string, object> dictionary, string key, float defaultValue = 0)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }

            try
            {
                return float.TryParse(value.ToString(), out var result) ? result : defaultValue;
            }
            catch (Exception e)
            {
                Log.Warning($"Can't convert value of key '{key}' to string. Exception: {e}");
                return defaultValue;
            }
        }
        
        public static float GeLong(this Dictionary<string, object> dictionary, string key, float defaultValue = 0)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }

            try
            {
                return Convert.ToInt64(value);
            }
            catch (Exception e)
            {
                Log.Warning($"Can't convert value of key '{key}' to string. Exception: {e}");
                return defaultValue;
            }
        }

        public static T GetEnum<T>(this Dictionary<string, object> dictionary, string key, T defaultValue = default(T)) where T : Enum
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
            {
                // Log.Warning($"No value found for key: {key}.");
                return defaultValue;
            }
            
            try
            {
                var enumValue = value?.ToString();
                return (T)Enum.Parse(typeof(T), enumValue, true);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
        
        public static List<T> GetList<T>(this Dictionary<string, object> dict, string prefix)
        {
            List<T> result = new List<T>();

            if (dict == null || string.IsNullOrEmpty(prefix))
            {
                Log.Warning("Dictionary is null or prefix is empty");
                return result;
            }

            foreach (var pair in dict.Where(pair => pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var value = (T)Convert.ChangeType(pair.Value, typeof(T));
                    result.Add(value);
                }
                catch
                {
                    Log.Warning($"Cannot convert value of key '{pair.Key}' to type {typeof(T).Name}");
                }
            }

            return result;
        }
    }
}