using UnityEngine;
using DucMinh;

/// <summary>
/// Example: How to use Storage Migration System with persistent config
/// </summary>
public class StorageMigrationExample : MonoBehaviour
{
    void Start()
    {
        ExampleAutoLoadFromConfig();
    }

    /// <summary>
    /// Example 1: Auto-load từ saved config
    /// Lần đầu chạy app sẽ dùng LocalStorage
    /// Sau khi migrate, lần sau app sẽ tự động load đúng storage type
    /// </summary>
    void ExampleAutoLoadFromConfig()
    {
        StorageService.Init();
        
        Debug.Log($"Current storage: {StorageService.GetCurrentStorageType()}");
        
        var savedType = StorageMigrator.GetSavedStorageType();
        if (savedType.HasValue)
        {
            Debug.Log($"Previously saved storage type: {savedType.Value}");
        }
        else
        {
            Debug.Log("No saved config, using default LocalStorage");
        }
    }

    /// <summary>
    /// Example 2: Migrate lần đầu
    /// App đang dùng LocalStorage, migrate sang JsonStorage
    /// Config sẽ được lưu tự động
    /// </summary>
    void ExampleFirstTimeMigration()
    {
        Debug.Log("=== First Time Migration ===");
        
        StorageService.SetInt("player_level", 10);
        StorageService.SetInt("player_gold", 1000);
        StorageService.SetString("player_name", "Hero");
        
        Debug.Log($"Before migration - Storage: {StorageService.GetCurrentStorageType()}");
        Debug.Log($"Saved config: {StorageMigrator.GetSavedStorageType()}");
        
        bool success = StorageService.MigrateTo(
            StorageMigrator.StorageType.JsonStorage,
            fileName: "game_data.json"
        );
        
        if (success)
        {
            Debug.Log("Migration successful!");
            Debug.Log($"After migration - Storage: {StorageService.GetCurrentStorageType()}");
            Debug.Log($"Saved config: {StorageMigrator.GetSavedStorageType()}");
            
            int level = StorageService.GetInt("player_level");
            Debug.Log($"Data preserved - Level: {level}");
        }
    }

    /// <summary>
    /// Example 3: Lần sau khởi động app
    /// App sẽ tự động load JsonStorage từ config
    /// KHÔNG CẦN migrate lại
    /// </summary>
    void ExampleSubsequentStartup()
    {
        Debug.Log("=== Subsequent App Startup ===");
        
        StorageService.Init();
        
        Debug.Log($"Auto-loaded storage: {StorageService.GetCurrentStorageType()}");
        
        int level = StorageService.GetInt("player_level");
        int gold = StorageService.GetInt("player_gold");
        string name = StorageService.GetString("player_name");
        
        Debug.Log($"Level: {level}, Gold: {gold}, Name: {name}");
    }

    /// <summary>
    /// Example 4: Check nếu cần migrate
    /// Useful khi có version update
    /// </summary>
    void ExampleCheckAndMigrateIfNeeded()
    {
        Debug.Log("=== Check and Migrate If Needed ===");
        
        var currentType = StorageMigrator.GetSavedStorageType();
        var targetType = StorageMigrator.StorageType.EncryptionStorage;
        
        if (!currentType.HasValue || currentType.Value != targetType)
        {
            Debug.Log($"Need to migrate from {currentType} to {targetType}");
            
            bool success = StorageService.MigrateTo(
                targetType,
                fileName: "encrypted_data.json",
                encryptionKey: SystemInfo.deviceUniqueIdentifier
            );
            
            if (success)
            {
                Debug.Log("Migration completed!");
            }
        }
        else
        {
            Debug.Log($"Already using {targetType}, no migration needed");
            StorageService.Init();
        }
    }

    /// <summary>
    /// Example 5: Version-based migration
    /// Sử dụng version để quyết định migrate
    /// </summary>
    void ExampleVersionBasedMigration()
    {
        Debug.Log("=== Version-Based Migration ===");
        
        StorageService.Init();
        
        int currentVersion = StorageService.GetInt("_app_version", 0);
        int targetVersion = 2;
        
        if (currentVersion < targetVersion)
        {
            Debug.Log($"Upgrading from version {currentVersion} to {targetVersion}");
            
            switch (currentVersion)
            {
                case 0:
                    Debug.Log("Version 0 -> 1: Migrate to JsonStorage");
                    if (StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage, "v1_data.json"))
                    {
                        StorageService.SetInt("_app_version", 1);
                        currentVersion = 1;
                    }
                    goto case 1;
                    
                case 1:
                    Debug.Log("Version 1 -> 2: Add encryption");
                    if (StorageService.MigrateTo(
                        StorageMigrator.StorageType.EncryptionStorage,
                        "v2_encrypted.json",
                        SystemInfo.deviceUniqueIdentifier))
                    {
                        StorageService.SetInt("_app_version", 2);
                    }
                    break;
            }
            
            Debug.Log($"Migration completed to version {StorageService.GetInt("_app_version")}");
        }
        else
        {
            Debug.Log($"Already at version {currentVersion}, no migration needed");
        }
    }

    /// <summary>
    /// Example 6: Clear config và reset về LocalStorage
    /// Useful cho testing hoặc reset app
    /// </summary>
    void ExampleResetToDefault()
    {
        Debug.Log("=== Reset to Default Storage ===");
        
        Debug.Log($"Before reset - Type: {StorageMigrator.GetSavedStorageType()}");
        
        StorageMigrator.ClearStorageConfig();
        
        Debug.Log($"After reset - Type: {StorageMigrator.GetSavedStorageType()}");
        
        StorageService.Init();
        
        Debug.Log($"Reinitialized with: {StorageService.GetCurrentStorageType()}");
    }

    /// <summary>
    /// Example 7: Platform-specific storage
    /// Editor dùng JsonStorage, Build dùng EncryptionStorage
    /// </summary>
    void ExamplePlatformSpecificStorage()
    {
        Debug.Log("=== Platform-Specific Storage ===");
        
        var savedType = StorageMigrator.GetSavedStorageType();
        
#if UNITY_EDITOR
        var targetType = StorageMigrator.StorageType.JsonStorage;
        var fileName = "editor_data.json";
        string encryptionKey = null;
#else
        var targetType = StorageMigrator.StorageType.EncryptionStorage;
        var fileName = "game_data.json";
        string encryptionKey = SystemInfo.deviceUniqueIdentifier;
#endif

        if (!savedType.HasValue || savedType.Value != targetType)
        {
            Debug.Log($"Migrating to platform-specific storage: {targetType}");
            StorageService.MigrateTo(targetType, fileName, encryptionKey);
        }
        else
        {
            Debug.Log($"Already using correct platform storage: {targetType}");
            StorageService.Init();
        }
    }

    /// <summary>
    /// Example 8: Timeline của migration workflow
    /// </summary>
    void ExampleMigrationTimeline()
    {
        Debug.Log("=== Migration Timeline ===");
        
        Debug.Log("\n--- Day 1: First Launch ---");
        Debug.Log("App starts with LocalStorage (default)");
        Debug.Log("User plays game, data saved to PlayerPrefs");
        
        Debug.Log("\n--- Day 2: App Update Released ---");
        Debug.Log("Update includes migration to JsonStorage");
        Debug.Log("On app start, Init() auto-loads from saved config");
        Debug.Log("Config says LocalStorage -> app loads LocalStorage");
        Debug.Log("App code calls MigrateTo(JsonStorage)");
        Debug.Log("Migration happens, config saved: JsonStorage");
        
        Debug.Log("\n--- Day 3: User Restarts App ---");
        Debug.Log("Init() auto-loads from saved config");
        Debug.Log("Config says JsonStorage -> app loads JsonStorage");
        Debug.Log("NO MIGRATION NEEDED!");
        Debug.Log("User continues playing with all data intact");
        
        Debug.Log("\n--- Day 10: Another Update ---");
        Debug.Log("Update adds encryption");
        Debug.Log("Init() loads JsonStorage from config");
        Debug.Log("App code checks version and migrates to EncryptionStorage");
        Debug.Log("Config updated: EncryptionStorage");
        
        Debug.Log("\n--- Day 11: User Restarts ---");
        Debug.Log("Init() loads EncryptionStorage from config");
        Debug.Log("NO MIGRATION NEEDED!");
        Debug.Log("Everything works seamlessly");
    }

    /// <summary>
    /// Example 9: Check migration status
    /// </summary>
    void ExampleCheckMigrationStatus()
    {
        Debug.Log("=== Migration Status ===");
        
        var savedType = StorageMigrator.GetSavedStorageType();
        var currentType = StorageService.GetCurrentStorageType();
        
        Debug.Log($"Saved in config: {savedType?.ToString() ?? "None"}");
        Debug.Log($"Currently using: {currentType}");
        
        var allKeys = StorageService.GetAllKeys();
        Debug.Log($"Total keys: {allKeys.Count}");
        
        var storage = StorageService.GetCurrentStorage();
        if (storage is JsonStorage jsonStorage)
        {
            Debug.Log($"JsonStorage - IsDirty: {jsonStorage.IsDirty}");
            Debug.Log($"JsonStorage - AutoSave: {jsonStorage.IsAutoSaveEnabled}");
        }
    }
}
