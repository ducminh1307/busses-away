using System;
using System.Collections.Generic;

namespace DucMinh
{
    public static class StorageService
    {
        private static IDataStorage __dataStorage;
        private static IDataStorage _dataStorage
        {
            get
            {
                if (__dataStorage == null)
                {
                    __dataStorage = StorageMigrator.LoadStorageFromConfig();
                    Log.Debug($"Auto-loaded storage from config: {__dataStorage.GetType().Name}");
                }
                return __dataStorage;
            }
        }

        public static void Init(IDataStorage dataStorage = null, bool autoMigrate = true)
        {
            if (dataStorage == null)
            {
                __dataStorage = StorageMigrator.LoadStorageFromConfig();
                Log.Debug($"Initialized with saved config: {__dataStorage.GetType().Name}");
                return;
            }

            if (autoMigrate && __dataStorage != null && __dataStorage != dataStorage)
            {
                try
                {
                    Log.Debug($"Auto-migrating from {__dataStorage.GetType().Name} to {dataStorage.GetType().Name}");
                    var keys = __dataStorage.GetAllKeys();

                    if (keys != null && keys.Count > 0)
                    {
                        if (StorageMigrator.MigrateWithBackup(__dataStorage, dataStorage, keys))
                        {
                            Log.Debug("Auto-migration successful");
                        }
                        else
                        {
                            Log.Error("Auto-migration failed, keeping old storage");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Auto-migration error: {e.Message}");
                    return;
                }
            }

            __dataStorage = dataStorage;
        }

        public static bool MigrateTo(StorageMigrator.StorageType targetType, string fileName = null, string encryptionKey = null)
        {
            try
            {
                var newStorage = StorageMigrator.CreateStorageByType(targetType, fileName, encryptionKey);
                var currentKeys = GetAllKeys();

                if (currentKeys == null || currentKeys.Count == 0)
                {
                    Log.Debug("No data to migrate, switching to new storage");
                    __dataStorage = newStorage;
                    return true;
                }

                Log.Debug($"Migrating {currentKeys.Count} keys to {targetType}");

                if (StorageMigrator.MigrateWithBackup(_dataStorage, newStorage, currentKeys))
                {
                    __dataStorage = newStorage;
                    StorageMigrator.SetStorageType(newStorage, targetType);
                    Log.Debug($"Successfully migrated to {targetType}");
                    return true;
                }
                else
                {
                    Log.Error($"Failed to migrate to {targetType}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Migration to {targetType} failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public static bool ManualMigrate(IDataStorage targetStorage, List<string> keys = null)
        {
            try
            {
                if (keys == null)
                {
                    keys = GetAllKeys();
                }

                return StorageMigrator.MigrateWithBackup(_dataStorage, targetStorage, keys);
            }
            catch (Exception e)
            {
                Log.Error($"Manual migration failed: {e.Message}");
                return false;
            }
        }

        public static List<string> GetAllKeys()
        {
            try
            {
                return _dataStorage.GetAllKeys();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get all keys: {e.Message}");
                return new List<string>();
            }
        }

        public static IDataStorage GetCurrentStorage()
        {
            return _dataStorage;
        }

        public static string GetCurrentStorageType()
        {
            return _dataStorage?.GetType().Name ?? "None";
        }

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

        public static void Save()
        {
            if (_dataStorage is JsonStorage jsonStorage)
            {
                jsonStorage.Save();
            }
        }
    }
}