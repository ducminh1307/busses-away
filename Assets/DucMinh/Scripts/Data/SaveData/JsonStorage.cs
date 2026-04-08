using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DucMinh
{
    public class JsonStorage : IDataStorage
    {
        private string _dataFileName = "data.json";
        private string path => Path.Combine(Application.persistentDataPath, _dataFileName);
        private string tempPath => path + ".tmp";
        private string backupPath => path + ".backup";

        private Dictionary<string, object> _cache = new();
        private bool _isDirty = false;
        private bool _autoSave = true;

        /// <summary>
        /// Creates a new JsonStorage instance
        /// </summary>
        /// <param name="dataFileName">Optional custom file name</param>
        /// <param name="autoSave">If true, automatically saves after each Set operation. If false, call Save() manually</param>
        public JsonStorage(string dataFileName = null, bool autoSave = true)
        {
            if (!string.IsNullOrEmpty(dataFileName))
            {
                _dataFileName = dataFileName;
            }
            _autoSave = autoSave;
            Read();
        }

        #region Read/Write Operations

        private void Read()
        {
            if (File.Exists(path))
            {
                if (TryReadFromFile(path))
                {
                    Log.Debug($"Loaded save data from: {path}");
                    return;
                }

                Log.Warning($"Main save file corrupted, attempting to restore from backup");
                if (File.Exists(backupPath) && TryReadFromFile(backupPath))
                {
                    Log.Debug($"Successfully restored from backup: {backupPath}");
                    Save();
                    return;
                }
            }

            Log.Debug($"No existing save data found at: {path}, starting fresh");
            _cache = new Dictionary<string, object>();
            _isDirty = false;
        }

        private bool TryReadFromFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Warning($"Save file is empty: {filePath}");
                    return false;
                }

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (data == null)
                {
                    Log.Warning($"Failed to deserialize save file: {filePath}");
                    return false;
                }

                _cache = data;
                _isDirty = false;
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to read json file: {filePath}\n{e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private void Write()
        {
            if (!_isDirty)
            {
                return;
            }

            try
            {
                var json = JsonConvert.SerializeObject(_cache);

                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);
                }

                File.WriteAllText(tempPath, json);

                if (!File.Exists(tempPath))
                {
                    throw new IOException("Failed to write temp file");
                }

                File.Copy(tempPath, path, true);
                File.Delete(tempPath);

                _isDirty = false;
                Log.Debug($"Successfully saved data to: {path}");
            }
            catch (Exception e)
            {
                Log.Error($"Cannot write json file: {path}\n{e.Message}\n{e.StackTrace}");

                if (File.Exists(backupPath))
                {
                    try
                    {
                        File.Copy(backupPath, path, true);
                        Log.Debug($"Restored from backup after write failure");
                    }
                    catch (Exception restoreException)
                    {
                        Log.Error($"Failed to restore from backup: {restoreException.Message}");
                    }
                }

                throw;
            }
        }

        public void Save()
        {
            Write();
        }

        public async Task SaveAsync()
        {
            await Task.Run(Write);
        }

        public void SetAutoSave(bool enabled)
        {
            _autoSave = enabled;

            if (_autoSave && _isDirty)
            {
                Save();
            }
        }

        #endregion

        #region Set

        public void SetInt(string key, int value)
        {
            if (_cache.TryGetValue(key, out var existingValue) && existingValue is int existing && existing == value)
            {
                return;
            }

            _cache[key] = value;
            _isDirty = true;

            if (_autoSave)
            {
                Write();
            }
        }

        public void SetFloat(string key, float value)
        {
            if (_cache.TryGetValue(key, out var existingValue) && existingValue is float existing && Math.Abs(existing - value) < float.Epsilon)
            {
                return;
            }

            _cache[key] = value;
            _isDirty = true;

            if (_autoSave)
            {
                Write();
            }
        }

        public void SetString(string key, string value)
        {
            if (_cache.TryGetValue(key, out var existingValue) && existingValue is string existing && existing == value)
            {
                return;
            }

            _cache[key] = value;
            _isDirty = true;

            if (_autoSave)
            {
                Write();
            }
        }

        public void SetBool(string key, bool value)
        {
            if (_cache.TryGetValue(key, out var existingValue) && existingValue is bool existing && existing == value)
            {
                return;
            }

            _cache[key] = value;
            _isDirty = true;

            if (_autoSave)
            {
                Write();
            }
        }

        public void SetDateTime(string key, DateTime value)
        {
            return;
            // var timestamp = TimeHelper.ToUnixTimestamp(value).ToString();
            //
            // if (_cache.TryGetValue(key, out var existingValue) && existingValue is string existing && existing == timestamp)
            // {
            //     return;
            // }
            //
            // _cache[key] = timestamp;
            // _isDirty = true;
            //
            // if (_autoSave)
            // {
            //     Write();
            // }
        }

        #endregion

        #region Get

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                if (value is int intValue) return intValue;
                if (int.TryParse(value.ToString(), out var result)) return result;
            }
            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                if (value is float floatValue) return floatValue;
                if (value is double doubleValue) return (float)doubleValue;
                if (float.TryParse(value.ToString(), out var result)) return result;
            }
            return defaultValue;
        }

        public string GetString(string key, string defaultValue = null)
        {
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString();
            }
            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_cache.TryGetValue(key, out var value) && value != null)
            {
                if (value is bool boolValue) return boolValue;
                if (bool.TryParse(value.ToString(), out var result)) return result;

                if (int.TryParse(value.ToString(), out var intResult)) return intResult == 1;
            }
            return defaultValue;
        }

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            // if (_cache.TryGetValue(key, out var value) && value != null)
            // {
            //     if (long.TryParse(value.ToString(), out var timestamp))
            //     {
            //         return TimeHelper.FromUnixTimestamp(timestamp);
            //     }
            // }
            return defaultValue;
        }

        #endregion

        #region Utility

        public bool HasKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public void Delete(string key)
        {
            if (_cache.Remove(key))
            {
                _isDirty = true;

                if (_autoSave)
                {
                    Write();
                }
            }
        }

        public void DeleteAll()
        {
            _cache.Clear();
            _isDirty = false;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                Log.Debug($"Deleted all save data");
            }
            catch (Exception e)
            {
                Log.Error($"Failed to delete save files: {e.Message}");
            }
        }

        public bool IsDirty => _isDirty;
        public bool IsAutoSaveEnabled => _autoSave;

        public List<string> GetAllKeys()
        {
            return new List<string>(_cache.Keys);
        }

        #endregion
    }
}