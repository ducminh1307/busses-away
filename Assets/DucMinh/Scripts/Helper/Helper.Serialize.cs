using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace DucMinh
{
    public static partial class Helper
    {
        private const char ListSeparator = '|';
        private const char KeyValueSeparator = ':';
        private const char DictionarySeparator = ',';
        private const char OpenBracket = '[';
        private const char CloseBracket = ']';

        public static string RemoveOpenClose(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            if (!str.StartsWith(OpenBracket) || !str.EndsWith(CloseBracket) || str.Length < 2)
            {
                throw new SerializationException("Invalid format: missing [ ] brackets");
            }

            return str.Substring(1, str.Length - 2);
        }
        
        public static string AddOpenClose(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            return $"{OpenBracket}{str}{CloseBracket}";
        }

        public static string Serialize<T>(List<T> list, char separator = ListSeparator)
        {
            if (list.IsNullOrEmpty()) throw new AggregateException(nameof(list));
            try
            {
                var content = string.Join(separator, list.Select(x => x.ToString() ?? ""));
                return $"{OpenBracket}{content}{CloseBracket}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static List<T> Deserialize<T>(string content, char separator = ListSeparator)
        {
            if (string.IsNullOrEmpty(content)) return null;
            try
            {
                var data = content.RemoveOpenClose();
                return data.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => (T)Convert.ChangeType(x, typeof(T))).ToList();
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to deserialize to List<{typeof(T).Name}>: {e.Message}", e);
            }
        }

        public static string Serialize(Dictionary<string, object> dict, char keyValueSeparator = KeyValueSeparator, char separator = DictionarySeparator)
        {
            if (dict == null) throw new ArgumentNullException(nameof(dict));
            try
            {
                var pairs = dict.Select(kvp =>
                {
                    string valueStr;
                    if (kvp.Value is List<string> stringList)
                    {
                        valueStr = Serialize(stringList);
                    }
                    else
                    {
                        valueStr = kvp.Value?.ToString() ?? "";
                    }
                    return $"{kvp.Key}{keyValueSeparator}{valueStr}";
                });
                var content = string.Join(separator, pairs);
                return $"{OpenBracket}{content}{CloseBracket}";
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to serialize Dictionary<string, object>: {e.Message}", e);
            }
        }
        
        public static string Serialize(Action<Dictionary<string, object>> callback, char keyValueSeparator = KeyValueSeparator, char separator = DictionarySeparator)
        {
            var dict = new Dictionary<string, object>();
            callback.Invoke(dict);
            try
            {
                var pairs = dict.Select(kvp =>
                {
                    string valueStr;
                    if (kvp.Value is List<string> stringList)
                    {
                        valueStr = Serialize(stringList);
                    }
                    else
                    {
                        valueStr = kvp.Value?.ToString() ?? "";
                    }
                    return $"{kvp.Key}{keyValueSeparator}{valueStr}";
                });
                var content = string.Join(separator, pairs);
                return $"{OpenBracket}{content}{CloseBracket}";
            }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to serialize Dictionary<string, object>: {e.Message}", e);
            }
        }

        public static void Deserialize(string data, Action<Dictionary<string, object>> callback, char keyValueSeparator = KeyValueSeparator,
            char separator = DictionarySeparator)
        {
            var result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(data)) callback?.Invoke(null);
            try
            {
                var content = data.RemoveOpenClose();
                foreach (var item in content.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = item.Split(new[] { keyValueSeparator }, 2, StringSplitOptions.None);
                    if (parts.Length != 2) throw new SerializationException("Invalid key-value format");

                    var key = parts[0];
                    object value = parts[1];

                    result[key] = value;
                }
                callback?.Invoke(result);
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize to Dictionary<string, object>: {ex.Message}", ex);
            }
        }
        
        public static Dictionary<string, object> Deserialize(string data, char keyValueSeparator = KeyValueSeparator, char separator = DictionarySeparator)
        {
            var result = new Dictionary<string, object>();
            try
            {
                var content = data.RemoveOpenClose();
                // Log.Debug(content);
                foreach (var item in content.Split(separator))
                {
                    var parts = item.Split(new[] { keyValueSeparator }, 2, StringSplitOptions.None);
                    if (parts.Length != 2) throw new SerializationException("Invalid key-value format");

                    var key = parts[0];
                    object value = parts[1];

                    result[key] = value;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new SerializationException($"Failed to deserialize to Dictionary<string, object>: {ex.Message}", ex);
            }
        }
    }
}