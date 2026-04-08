using System;
using System.Collections.Generic;
using System.IO;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this List<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static T Get<T>(this List<T> collection, int index, T defaultValue = default)
        {
            if (index < 0 || index >= collection.Count) return default;
            return collection[index];
        }

        public static string ConvertToString<T>(this List<T> collection, string separator = ",")
        {
            if (collection.IsNullOrEmpty())
                return StringNull;

            return string.Join(separator, collection);
        }

        public static bool TryRemoveValue<T>(this List<T> collection, T value)
        {
            if (collection.IsNullOrEmpty())
                return false;

            if (collection.Contains(value))
            {
                collection.Remove(value);
                return true;
            }

            return false;
        }

        public static bool CheckAdd<T>(this List<T> list, T value)
        {
            if (list.Contains(value)) return false;

            list.Add(value);
            return true;
        }

        public static void TryAddRange<T>(this List<T> list, List<T> values)
        {
            foreach (var value in values)
            {
                list.CheckAdd(value);
            }
        }

        public static void GenerateCode<T>(this List<T> data, string fileName, string nameSpace, Func<string> generateCodeContent)
        {
            try
            {
                var filePath = GetFilePath(fileName, nameSpace, "cs");
                var codeContent = generateCodeContent();

                if (string.IsNullOrEmpty(codeContent))
                {
                    Log.Warning($"No code content generated for file: {filePath}");
                    return;
                }

                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, codeContent);
                Log.Debug($"Code file generated successfully at: {filePath}");
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh(); // Refresh để Unity nhận file mới
#endif
            }
            catch (Exception ex)
            {
                Log.Warning($"Error generating code file {fileName} in namespace {nameSpace}: {ex.Message}");
            }
        }

        public static bool AreAllDistinct<T>(this List<T> list)
        {
            if (list.IsNullOrEmpty()) return true;

            var uniqueSet = new HashSet<T>();
            foreach (var item in list)
            {
                if (!uniqueSet.Add(item)) return false;
            }
            return true;
        }
    }
}