using System;
using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    public static class StorageMigrator
    {
        public enum StorageType
        {
            LocalStorage,
            JsonStorage,
            EncryptionStorage
        }

        private const string MIGRATION_VERSION_KEY = "_storage_migration_version";
        private const string STORAGE_TYPE_KEY = "_storage_type";

        private const string PREF_STORAGE_TYPE = "DucMinh_StorageType";
        private const string PREF_STORAGE_FILE = "DucMinh_StorageFile";
        private const string PREF_ENCRYPTION_KEY = "DucMinh_EncryptionKey";

        public static bool MigrateData(IDataStorage fromStorage, IDataStorage toStorage, List<string> keys = null)
        {
            try
            {
                Log.Debug($"Starting data migration from {fromStorage.GetType().Name} to {toStorage.GetType().Name}");

                if (keys == null || keys.Count == 0)
                {
                    keys = GetAllKeysFromStorage(fromStorage);
                }

                if (keys == null || keys.Count == 0)
                {
                    Log.Warning("No keys found to migrate");
                    return true;
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var key in keys)
                {
                    if (key == MIGRATION_VERSION_KEY || key == STORAGE_TYPE_KEY)
                    {
                        continue;
                    }

                    try
                    {
                        MigrateKey(fromStorage, toStorage, key);
                        successCount++;
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to migrate key '{key}': {e.Message}");
                        failCount++;
                    }
                }

                Log.Debug($"Migration completed: {successCount} succeeded, {failCount} failed");
                return failCount == 0;
            }
            catch (Exception e)
            {
                Log.Error($"Migration failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private static void MigrateKey(IDataStorage fromStorage, IDataStorage toStorage, string key)
        {
            if (!fromStorage.HasKey(key))
            {
                return;
            }

            var stringValue = fromStorage.GetString(key);
            if (stringValue != null)
            {
                if (int.TryParse(stringValue, out int intValue))
                {
                    toStorage.SetInt(key, intValue);
                    return;
                }

                if (float.TryParse(stringValue, out float floatValue))
                {
                    toStorage.SetFloat(key, floatValue);
                    return;
                }

                if (bool.TryParse(stringValue, out bool boolValue))
                {
                    toStorage.SetBool(key, boolValue);
                    return;
                }

                if (long.TryParse(stringValue, out long longValue))
                {
                    return;
                    // try
                    // {
                    //     var dateTime = TimeHelper.FromUnixTimestamp(longValue);
                    //     if (dateTime.Year > 2000 && dateTime.Year < 2100)
                    //     {
                    //         toStorage.SetDateTime(key, dateTime);
                    //         return;
                    //     }
                    // }
                    // catch
                    // {
                    // }
                }

                toStorage.SetString(key, stringValue);
            }
        }

        private static List<string> GetAllKeysFromStorage(IDataStorage storage)
        {
            var keys = new List<string>();

            if (storage is LocalStorage)
            {
                return GetAllPlayerPrefsKeys();
            }

            if (storage is JsonStorage jsonStorage)
            {
                return GetAllJsonStorageKeys(jsonStorage);
            }

            Log.Warning($"Cannot auto-detect keys for storage type: {storage.GetType().Name}");
            return keys;
        }

        private static List<string> GetAllPlayerPrefsKeys() => new();

        private static List<string> GetAllJsonStorageKeys(JsonStorage jsonStorage)
        {
            var keys = new List<string>();
            return keys;
        }

        public static void SetStorageType(IDataStorage storage, StorageType type)
        {
            storage.SetString(STORAGE_TYPE_KEY, type.ToString());
            SaveStorageTypeConfig(type);
        }

        public static StorageType? GetStorageType(IDataStorage storage)
        {
            var typeString = storage.GetString(STORAGE_TYPE_KEY);
            if (Enum.TryParse<StorageType>(typeString, out var type))
            {
                return type;
            }
            return null;
        }

        public static void SetMigrationVersion(IDataStorage storage, int version)
        {
            storage.SetInt(MIGRATION_VERSION_KEY, version);
        }

        public static int GetMigrationVersion(IDataStorage storage)
        {
            return storage.GetInt(MIGRATION_VERSION_KEY, 0);
        }

        public static bool NeedsMigration(IDataStorage currentStorage, StorageType targetType)
        {
            var currentType = GetStorageType(currentStorage);
            if (currentType == null)
            {
                return false;
            }

            return currentType != targetType;
        }

        public static IDataStorage CreateStorageByType(StorageType type, string fileName = null, string encryptionKey = null)
        {
            switch (type)
            {
                case StorageType.LocalStorage:
                    return new LocalStorage();

                case StorageType.JsonStorage:
                    return new JsonStorage(fileName);

                case StorageType.EncryptionStorage:
                    if (string.IsNullOrEmpty(encryptionKey))
                    {
                        encryptionKey = SystemInfo.deviceUniqueIdentifier;
                    }
                    var baseStorage = new JsonStorage(fileName);
                    return new EncryptionStorage(baseStorage, encryptionKey);

                default:
                    throw new ArgumentException($"Unknown storage type: {type}");
            }
        }

        public static bool MigrateWithBackup(IDataStorage fromStorage, IDataStorage toStorage, List<string> keys = null)
        {
            try
            {
                var backupStorage = new JsonStorage("migration_backup.json");

                Log.Debug("Creating backup before migration...");
                if (!MigrateData(fromStorage, backupStorage, keys))
                {
                    Log.Error("Failed to create backup, aborting migration");
                    return false;
                }

                Log.Debug("Migrating data to new storage...");
                if (!MigrateData(fromStorage, toStorage, keys))
                {
                    Log.Error("Migration failed, restoring from backup...");
                    MigrateData(backupStorage, fromStorage, keys);
                    return false;
                }

                Log.Debug("Migration successful, cleaning up backup...");
                backupStorage.DeleteAll();
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Migration with backup failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private static void SaveStorageTypeConfig(StorageType type, string fileName = null, string encryptionKey = null)
        {
            PlayerPrefs.SetString(PREF_STORAGE_TYPE, type.ToString());

            if (!string.IsNullOrEmpty(fileName))
            {
                PlayerPrefs.SetString(PREF_STORAGE_FILE, fileName);
            }

            if (!string.IsNullOrEmpty(encryptionKey))
            {
                PlayerPrefs.SetString(PREF_ENCRYPTION_KEY, encryptionKey);
            }

            PlayerPrefs.Save();
            Log.Debug($"Saved storage config: {type}, file: {fileName ?? "default"}");
        }

        public static IDataStorage LoadStorageFromConfig()
        {
            var typeString = PlayerPrefs.GetString(PREF_STORAGE_TYPE, null);

            if (string.IsNullOrEmpty(typeString))
            {
                Log.Debug("No saved storage config found, using LocalStorage");
                return new LocalStorage();
            }

            if (Enum.TryParse<StorageType>(typeString, out var type))
            {
                var fileName = PlayerPrefs.GetString(PREF_STORAGE_FILE, null);
                var encryptionKey = PlayerPrefs.GetString(PREF_ENCRYPTION_KEY, null);

                Log.Debug($"Loading storage from config: {type}, file: {fileName ?? "default"}");
                return CreateStorageByType(type, fileName, encryptionKey);
            }

            Log.Warning($"Unknown storage type in config: {typeString}, using LocalStorage");
            return new LocalStorage();
        }

        public static StorageType? GetSavedStorageType()
        {
            var typeString = PlayerPrefs.GetString(PREF_STORAGE_TYPE, null);
            if (string.IsNullOrEmpty(typeString))
            {
                return null;
            }

            if (Enum.TryParse<StorageType>(typeString, out var type))
            {
                return type;
            }

            return null;
        }

        public static void ClearStorageConfig()
        {
            PlayerPrefs.DeleteKey(PREF_STORAGE_TYPE);
            PlayerPrefs.DeleteKey(PREF_STORAGE_FILE);
            PlayerPrefs.DeleteKey(PREF_ENCRYPTION_KEY);
            PlayerPrefs.Save();
            Log.Debug("Cleared storage config");
        }
    }
}
