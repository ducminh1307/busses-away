using System;

namespace DucMinh
{
    public static class LocalDataService
    {
        private static IDataStorage __dataStorage;
        private static IDataStorage _dataStorage => __dataStorage ??= new LocalStorage();
        
        #region Set Data

        public static void SetInt(string key, int value)
        {
            _dataStorage.SetInt(key, value);
        }
        
        public static void SetFloat(string key, float value)
        {
            _dataStorage.SetFloat(key, value);
        }
        
        public static void SetString(string key, string value)
        {
            _dataStorage.SetString(key, value);
        }
        
        public static void SetBool(string key, bool value)
        {
            _dataStorage.SetBool(key, value);
        }
        
        public static void SetDateTime(string key, DateTime value)
        {
            _dataStorage.SetDateTime(key, value);
        }

        #endregion

        #region GetData

        public static int GetInt(string key, int defaultValue = 0)
        {
            return _dataStorage.GetInt(key, defaultValue);
        }
        
        public static float GetFloat(string key, float defaultValue = 0)
        {
            return _dataStorage.GetFloat(key, defaultValue);
        }
        
        public static string GetString(string key, string defaultValue = null)
        {
            return _dataStorage.GetString(key, defaultValue);
        }
        
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return _dataStorage.GetBool(key, defaultValue);
        }
        
        public static DateTime GetDateTime(string key, DateTime defaultValue = default)
        {
            return _dataStorage.GetDateTime(key, defaultValue);
        }

        #endregion

        public static bool HasKey(string key)
        {
            return _dataStorage.HasKey(key);
        }

        public static void Delete(string key)
        {
            _dataStorage.Delete(key);
        }

        public static void DeleteAll()
        {
            _dataStorage.DeleteAll();
        }
    }
}