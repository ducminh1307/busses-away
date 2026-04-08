#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DucMinh
{
    public class EditorJsonStorage : IDataStorage
    {
        private readonly string _filePath;
        private readonly string _tempPath;
        private readonly string _backupPath;

        private Dictionary<string, object> _cache = new Dictionary<string, object>();
        private bool _isDirty;

        /// <param name="absoluteFilePath">Đường dẫn tuyệt đối đến file JSON, ví dụ: Path.Combine(Application.dataPath, "../ProjectSettings/GeneratorPresets.json")</param>
        public EditorJsonStorage(string absoluteFilePath)
        {
            _filePath = absoluteFilePath;
            _tempPath = absoluteFilePath + ".tmp";
            _backupPath = absoluteFilePath + ".backup";
            Read();
        }

        // ─── Read / Write ────────────────────────────────────────────────────────

        private void Read()
        {
            if (File.Exists(_filePath))
            {
                if (TryReadFromFile(_filePath)) return;

                Debug.LogWarning($"[EditorJsonStorage] Main file corrupted, trying backup: {_backupPath}");
                if (File.Exists(_backupPath) && TryReadFromFile(_backupPath))
                {
                    Debug.Log("[EditorJsonStorage] Restored from backup.");
                    Save();
                    return;
                }
            }

            _cache = new Dictionary<string, object>();
            _isDirty = false;
        }

        private bool TryReadFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return false;

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data == null) return false;

                _cache = data;
                _isDirty = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorJsonStorage] Failed to read {path}: {e.Message}");
                return false;
            }
        }

        public void Save()
        {
            if (!_isDirty) return;

            try
            {
                // Đảm bảo thư mục tồn tại
                var dir = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);

                if (File.Exists(_filePath))
                    File.Copy(_filePath, _backupPath, true);

                File.WriteAllText(_tempPath, json);
                File.Copy(_tempPath, _filePath, true);
                File.Delete(_tempPath);

                _isDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorJsonStorage] Failed to save {_filePath}: {e.Message}");
            }
        }

        // ─── Set ────────────────────────────────────────────────────────────────

        public void SetInt(string key, int value)    { _cache[key] = value; _isDirty = true; Save(); }
        public void SetFloat(string key, float value) { _cache[key] = value; _isDirty = true; Save(); }
        public void SetBool(string key, bool value)  { _cache[key] = value; _isDirty = true; Save(); }

        public void SetString(string key, string value)
        {
            _cache[key] = value ?? "";
            _isDirty = true;
            Save();
        }

        public void SetDateTime(string key, DateTime value)
        {
            _cache[key] = value.ToString("O");
            _isDirty = true;
            Save();
        }

        // ─── Get ────────────────────────────────────────────────────────────────

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_cache.TryGetValue(key, out var v) && v != null)
            {
                if (v is int i) return i;
                if (int.TryParse(v.ToString(), out var r)) return r;
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            if (_cache.TryGetValue(key, out var v) && v != null)
            {
                if (v is float f) return f;
                if (v is double d) return (float)d;
                if (float.TryParse(v.ToString(), out var r)) return r;
            }
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = null)
        {
            if (_cache.TryGetValue(key, out var v) && v != null) return v.ToString();
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_cache.TryGetValue(key, out var v) && v != null)
            {
                if (v is bool b) return b;
                if (bool.TryParse(v.ToString(), out var r)) return r;
                if (int.TryParse(v.ToString(), out var i)) return i == 1;
            }
            return defaultValue;
        }

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            if (_cache.TryGetValue(key, out var v) && v != null &&
                DateTime.TryParse(v.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
                return result;
            return defaultValue;
        }

        // ─── Utility ────────────────────────────────────────────────────────────

        public bool HasKey(string key) => _cache.ContainsKey(key);

        public void Delete(string key)
        {
            if (_cache.Remove(key)) { _isDirty = true; Save(); }
        }

        public void DeleteAll()
        {
            _cache.Clear();
            _isDirty = false;
            try
            {
                if (File.Exists(_filePath)) File.Delete(_filePath);
                if (File.Exists(_backupPath)) File.Delete(_backupPath);
                if (File.Exists(_tempPath)) File.Delete(_tempPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorJsonStorage] DeleteAll failed: {e.Message}");
            }
        }

        public List<string> GetAllKeys() => new List<string>(_cache.Keys);

        public bool IsDirty => _isDirty;
    }
}
#endif