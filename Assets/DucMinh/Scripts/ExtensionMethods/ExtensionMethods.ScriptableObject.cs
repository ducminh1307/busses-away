using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Path = System.IO.Path;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void LoadCSV(this ScriptableObject so, string fileName, string nameSpace,
            Action<List<Dictionary<string, object>>> callback)
        {
            List<Dictionary<string, object>> data = new();
            try
            {
                var filePath = GetFilePath(fileName, nameSpace);
                if (!File.Exists(filePath))
                {
                    Log.Warning($"File not found at: {filePath}");
                    callback?.Invoke(data);
                    return;
                }

                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                {
                    Log.Warning($"File is empty: {filePath}");
                    callback?.Invoke(data);
                    return;
                }

                data = ParseData(lines);
                callback?.Invoke(data);
            }
            catch (Exception e)
            {
                Log.Error($"Error loading file {fileName}.csv in namespace {nameSpace}: {e.Message}");
                throw;
            }
        }

        public static void SaveToCSV<T>(this ScriptableObject so, List<T> data, string fileName, string nameSpace,
            string extension = "csv")
        {
            try
            {
                string filePath = GetFilePath(fileName, nameSpace, extension);
                string csvContent = ConvertToCSV(data);
                // Tạo thư mục nếu chưa tồn tại
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, csvContent);
                Log.Debug($"CSV file saved successfully at: {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error saving CSV file {fileName} in namespace {nameSpace}: {ex.Message}");
            }
        }

        public static string GetFilePath(this string fileName, string nameSpace, string extension = "csv")
        {
            if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
                extension = "." + extension;

            if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                fileName += extension;

            var fullname = Path.Combine(Application.dataPath, nameSpace, "Resources", fileName);
            return fullname;
        }

        private static List<Dictionary<string, object>> ParseData(string[] lines)
        {
            var result = new List<Dictionary<string, object>>();

            if (lines == null || lines.Length <= 1)
            {
                Log.Warning("No data lines to parse (file has only header or is empty).");
                return result;
            }

            var headers = SplitCSVLine(lines[0]);
            if (headers.Length == 0) return result;

            for (var i = 1; i < lines.Length; i++)
            {
                var values = SplitCSVLine(lines[i]);
                if (values.Length == 0) continue;

                var item = new Dictionary<string, object>();
                var hasData = false;
                for (var j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    item[headers[j]] = values[j];
                    hasData = true;
                }

                if (hasData)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static string ConvertToCSV<T>(List<T> data)
        {
            if (data == null || data.Count == 0)
                return "";

            StringBuilder csv = new StringBuilder();
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            if (fields.Length == 0)
            {
                Log.Error($"Type {typeof(T).Name} has no public instance fields. Cannot convert to CSV.");
                return "";
            }

            csv.AppendLine(string.Join(",", fields.Select(f => $"\"{f.Name}\"")));

            foreach (var item in data)
            {
                var values = new List<string>();
                foreach (var field in fields)
                {
                    object value = field.GetValue(item);
                    string csvValue = value != null ? value.ToString() : "";
                    if (csvValue.Contains(",") || csvValue.Contains("\""))
                    {
                        csvValue = $"\"{csvValue.Replace("\"", "\"\"")}\"";
                    }

                    values.Add(csvValue);
                }

                csv.AppendLine(string.Join(",", values));
            }

            return csv.ToString();
        }

        private static string[] SplitCSVLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            string currentValue = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && (i == 0 || line[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.Trim());
                    currentValue = "";
                    continue;
                }

                currentValue += c;
            }

            if (currentValue.Length > 0)
            {
                values.Add(currentValue.Trim());
            }

            return values.ToArray();
        }
    }
}