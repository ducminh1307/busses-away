using System;
using System.Collections.Generic;

namespace DucMinh
{
    public interface IDataStorage
    {
        void SetInt(string key, int value);
        void SetFloat(string key, float value);
        void SetString(string key, string value);
        void SetBool(string key, bool value);
        void SetDateTime(string key, DateTime value);
        
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0);
        string GetString(string key, string defaultValue = null);
        bool GetBool(string key, bool defaultValue = false);
        DateTime GetDateTime(string key, DateTime defaultValue = default);
        
        bool HasKey(string key);
        void Delete(string key);
        void DeleteAll();
        
        List<string> GetAllKeys();
    }
}