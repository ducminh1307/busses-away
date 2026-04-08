#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace DucMinh
{
    public class EditorStorage : IDataStorage
    {
        private readonly string _prefix;

        private const string KeyRegistryKey = "__keys__";
        private readonly List<string> _registeredKeys;

        public EditorStorage(string keyPrefix = "")
        {
            _prefix = keyPrefix;
            _registeredKeys = LoadKeyRegistry();
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

        private string Prefixed(string key) => _prefix + key;

        private List<string> LoadKeyRegistry()
        {
            var raw = EditorPrefs.GetString(Prefixed(KeyRegistryKey), "");
            if (string.IsNullOrEmpty(raw)) return new List<string>();

            var keys = new List<string>();
            foreach (var k in raw.Split('\n'))
            {
                if (!string.IsNullOrEmpty(k)) keys.Add(k);
            }
            return keys;
        }

        private void SaveKeyRegistry()
        {
            EditorPrefs.SetString(Prefixed(KeyRegistryKey), string.Join("\n", _registeredKeys));
        }

        private void RegisterKey(string key)
        {
            if (!_registeredKeys.Contains(key))
            {
                _registeredKeys.Add(key);
                SaveKeyRegistry();
            }
        }

        private void UnregisterKey(string key)
        {
            if (_registeredKeys.Remove(key))
            {
                SaveKeyRegistry();
            }
        }

        // ─── Set ────────────────────────────────────────────────────────────────

        public void SetInt(string key, int value)
        {
            EditorPrefs.SetInt(Prefixed(key), value);
            RegisterKey(key);
        }

        public void SetFloat(string key, float value)
        {
            EditorPrefs.SetFloat(Prefixed(key), value);
            RegisterKey(key);
        }

        public void SetString(string key, string value)
        {
            EditorPrefs.SetString(Prefixed(key), value ?? "");
            RegisterKey(key);
        }

        public void SetBool(string key, bool value)
        {
            EditorPrefs.SetBool(Prefixed(key), value);
            RegisterKey(key);
        }

        public void SetDateTime(string key, DateTime value)
        {
            EditorPrefs.SetString(Prefixed(key), value.ToString("O"));
            RegisterKey(key);
        }

        // ─── Get ────────────────────────────────────────────────────────────────

        public int GetInt(string key, int defaultValue = 0)
            => EditorPrefs.GetInt(Prefixed(key), defaultValue);

        public float GetFloat(string key, float defaultValue = 0)
            => EditorPrefs.GetFloat(Prefixed(key), defaultValue);

        public string GetString(string key, string defaultValue = null)
        {
            if (!EditorPrefs.HasKey(Prefixed(key))) return defaultValue;
            return EditorPrefs.GetString(Prefixed(key), defaultValue ?? "");
        }

        public bool GetBool(string key, bool defaultValue = false)
            => EditorPrefs.GetBool(Prefixed(key), defaultValue);

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            if (!EditorPrefs.HasKey(Prefixed(key))) return defaultValue;
            var raw = EditorPrefs.GetString(Prefixed(key), "");
            return DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result)
                ? result
                : defaultValue;
        }

        // ─── Utility ────────────────────────────────────────────────────────────

        public bool HasKey(string key) => EditorPrefs.HasKey(Prefixed(key));

        public void Delete(string key)
        {
            EditorPrefs.DeleteKey(Prefixed(key));
            UnregisterKey(key);
        }

        public void DeleteAll()
        {
            foreach (var key in new List<string>(_registeredKeys))
            {
                EditorPrefs.DeleteKey(Prefixed(key));
            }
            EditorPrefs.DeleteKey(Prefixed(KeyRegistryKey));
            _registeredKeys.Clear();
        }

        public List<string> GetAllKeys() => new(_registeredKeys);
    }
}
#endif