using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DucMinh.GenerateScript
{
    public enum NamingConvention
    {
        PascalCase,
        CamelCase,
        SnakeCase,
        KebabCase
    }

    /// <summary>
    /// Shared settings cho toàn bộ GenerateScript tool.
    /// Lưu tại ProjectSettings/GeneratorSettings.json — có thể commit lên SCM.
    /// </summary>
    [Serializable]
    public class GeneratorSettings
    {
        public string RootFolder { get; set; } = "Assets";
        public string DefaultNamespace { get; set; } = "MyGame";
        public NamingConvention NamingConvention { get; set; } = NamingConvention.PascalCase;

        // ─── Singleton ──────────────────────────────────────────────────────────

        private static GeneratorSettings _instance;
        private static readonly string FilePath = Path.Combine(
            Application.dataPath, "../ProjectSettings/GeneratorSettings.json");

        public static GeneratorSettings Instance
        {
            get
            {
                if (_instance == null) _instance = Load();
                return _instance;
            }
        }

        public static void ResetInstance() => _instance = null;

        // ─── Load / Save ────────────────────────────────────────────────────────

        private static GeneratorSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var settings = JsonConvert.DeserializeObject<GeneratorSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GeneratorSettings] Failed to load: {e.Message}");
            }
            return new GeneratorSettings();
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GeneratorSettings] Failed to save: {e.Message}");
            }
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        /// <summary>Trả về tên class theo convention đang được chọn.</summary>
        public string ApplyNaming(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            return NamingConvention switch
            {
                NamingConvention.PascalCase => ToPascalCase(name),
                NamingConvention.CamelCase  => ToCamelCase(name),
                NamingConvention.SnakeCase  => ToSnakeCase(name),
                NamingConvention.KebabCase  => ToKebabCase(name),
                _ => name
            };
        }

        private static string ToPascalCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToLower(s[0]) + s.Substring(1);
        }

        private static string ToSnakeCase(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s, "(?<=.)([A-Z])", "_$1").ToLower();

        private static string ToKebabCase(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s, "(?<=.)([A-Z])", "-$1").ToLower();
    }
}
