using System;
using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public class LocalStorage : IDataStorage
    {
        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public float GetFloat(string key, float defaultValue = 0)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public string GetString(string key, string defaultValue = null)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public void SetDateTime(string key, DateTime value)
        {
            return;
            // PlayerPrefs.SetString(key, TimeHelper.ToUnixTimestamp(value).ToString());
        }

        public DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            var value = PlayerPrefs.GetString(key);
            if (!long.TryParse(value, out var timestamp))
            {
                timestamp = 0;
            }

            return DateTime.Now;
            // return TimeHelper.FromUnixTimestamp(timestamp);
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public List<string> GetAllKeys() => new List<string>();
    }
}