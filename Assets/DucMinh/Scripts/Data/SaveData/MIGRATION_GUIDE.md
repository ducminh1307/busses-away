# Storage Migration System

## Tổng quan
Hệ thống migration cho phép bạn chuyển đổi giữa các loại storage khác nhau (LocalStorage, JsonStorage, EncryptionStorage) một cách an toàn với auto-backup và rollback.

## Các Storage Types
1. **LocalStorage** - Sử dụng Unity PlayerPrefs (đơn giản nhưng giới hạn)
2. **JsonStorage** - Lưu vào file JSON (linh hoạt, có backup, async support)
3. **EncryptionStorage** - Wrapper cho JsonStorage với encryption (bảo mật)

## Cách sử dụng

### 1. Auto-Migration khi Init
```csharp
// Khởi tạo với LocalStorage (default)
StorageService.Init();

// Sau đó muốn chuyển sang JsonStorage, auto-migrate data
var jsonStorage = new JsonStorage("save_data.json");
StorageService.Init(jsonStorage, autoMigrate: true); // Tự động migrate data từ LocalStorage sang JsonStorage
```

### 2. Manual Migration với MigrateTo
```csharp
// Migrate từ storage hiện tại sang JsonStorage
bool success = StorageService.MigrateTo(
    StorageMigrator.StorageType.JsonStorage,
    fileName: "game_save.json"
);

if (success)
{
    Debug.Log("Migration successful!");
}

// Migrate sang EncryptionStorage
success = StorageService.MigrateTo(
    StorageMigrator.StorageType.EncryptionStorage,
    fileName: "encrypted_save.json",
    encryptionKey: "my-secret-key-12345"
);
```

### 3. Migration với Custom Storage
```csharp
// Tạo custom storage
var customStorage = new JsonStorage("custom.json", autoSave: false);

// Get tất cả keys để migrate
var keys = StorageService.GetAllKeys();

// Manual migrate
bool success = StorageService.ManualMigrate(customStorage, keys);

if (success)
{
    // Chuyển sang sử dụng storage mới
    StorageService.Init(customStorage, autoMigrate: false);
}
```

### 4. Selective Migration (chỉ migrate một số keys)
```csharp
var fromStorage = StorageService.GetCurrentStorage();
var toStorage = new JsonStorage("new_save.json");

// Chỉ migrate một số keys cụ thể
var keysToMigrate = new List<string> 
{ 
    "player_level", 
    "player_gold", 
    "player_name" 
};

bool success = StorageMigrator.MigrateWithBackup(fromStorage, toStorage, keysToMigrate);
```

## Ví dụ thực tế

### Scenario 1: Chuyển từ PlayerPrefs sang JSON
```csharp
// Game đang dùng LocalStorage
void Start()
{
    StorageService.Init(); // LocalStorage default
    
    // Load game data
    int level = StorageService.GetInt("level", 1);
    int gold = StorageService.GetInt("gold", 0);
}

// Sau khi update game, muốn chuyển sang JsonStorage
void MigrateToJson()
{
    Debug.Log($"Current storage: {StorageService.GetCurrentStorageType()}");
    Debug.Log($"Keys to migrate: {StorageService.GetAllKeys().Count}");
    
    // Migrate tất cả data sang JsonStorage
    if (StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage, "game_data.json"))
    {
        Debug.Log("Successfully migrated to JsonStorage!");
        Debug.Log($"New storage: {StorageService.GetCurrentStorageType()}");
    }
    else
    {
        Debug.LogError("Migration failed! Still using old storage.");
    }
}
```

### Scenario 2: Thêm Encryption cho data hiện tại
```csharp
void AddEncryption()
{
    // Đang dùng JsonStorage
    var currentStorage = StorageService.GetCurrentStorage();
    Debug.Log($"Current: {currentStorage.GetType().Name}");
    
    // Migrate sang EncryptionStorage
    bool success = StorageService.MigrateTo(
        StorageMigrator.StorageType.EncryptionStorage,
        fileName: "encrypted_data.json",
        encryptionKey: SystemInfo.deviceUniqueIdentifier // Dùng device ID làm key
    );
    
    if (success)
    {
        Debug.Log("Data is now encrypted!");
        
        // Verify data còn nguyên
        int level = StorageService.GetInt("level");
        Debug.Log($"Level after encryption: {level}");
    }
}
```

### Scenario 3: Batch Operations với JsonStorage
```csharp
void SaveBatchData()
{
    // Init JsonStorage với autoSave = false để tối ưu performance
    var jsonStorage = new JsonStorage("data.json", autoSave: false);
    StorageService.Init(jsonStorage);
    
    // Batch set data - không write file từng cái
    StorageService.SetInt("level", 10);
    StorageService.SetInt("gold", 1000);
    StorageService.SetInt("gems", 50);
    StorageService.SetString("playerName", "Hero");
    
    // Manual save 1 lần
    StorageService.Save();
    
    Debug.Log("All data saved in one write operation!");
}
```

## Safety Features

### 1. Automatic Backup
Mỗi lần migrate, hệ thống tự động:
- Tạo backup file (`migration_backup.json`)
- Thử migrate sang storage mới
- Nếu thất bại → restore từ backup
- Nếu thành công → xóa backup

### 2. Atomic Write (JsonStorage)
- Ghi vào temp file trước
- Verify temp file thành công
- Move atomic sang file chính
- Không bao giờ corrupt file chính

### 3. Fallback to Backup
Nếu file chính corrupt:
- Tự động detect
- Restore từ `.backup` file
- Log warning để developer biết

### 4. Error Handling
```csharp
try 
{
    bool success = StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage);
    if (!success)
    {
        // Migration failed but data is safe
        Debug.LogError("Migration failed, keeping current storage");
    }
}
catch (Exception e)
{
    Debug.LogError($"Critical error: {e.Message}");
    // Current storage is still intact
}
```

## Best Practices

### 1. ✅ Migration khi app khởi động
```csharp
void Awake()
{
    #if UNITY_EDITOR
        // Editor dùng JsonStorage để dễ debug
        var storage = new JsonStorage("editor_data.json");
    #else
        // Build dùng EncryptionStorage để bảo mật
        var storage = new EncryptionStorage(
            new JsonStorage("game_data.json"),
            SystemInfo.deviceUniqueIdentifier
        );
    #endif
    
    StorageService.Init(storage, autoMigrate: true);
}
```

### 2. ✅ Check storage type before migration
```csharp
void MigrateIfNeeded()
{
    var currentType = StorageService.GetCurrentStorageType();
    
    if (currentType == "LocalStorage")
    {
        Debug.Log("Migrating from PlayerPrefs to JSON...");
        StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage);
    }
}
```

### 3. ✅ Use version tracking
```csharp
void CheckMigrationVersion()
{
    int version = StorageService.GetInt("_migration_version", 0);
    
    switch (version)
    {
        case 0: // First time or old version
            // Migrate to JsonStorage
            if (StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage))
            {
                StorageService.SetInt("_migration_version", 1);
            }
            break;
            
        case 1: // Already on JsonStorage
            // Maybe add encryption in future
            break;
    }
}
```

### 4. ❌ Tránh migrate quá thường xuyên
```csharp
// BAD
void Update()
{
    StorageService.MigrateTo(StorageMigrator.StorageType.JsonStorage); // Không làm vậy!
}

// GOOD
void Start()
{
    MigrateIfNeeded(); // Chỉ 1 lần khi start
}
```

## Utilities

### Get all keys
```csharp
var allKeys = StorageService.GetAllKeys();
foreach (var key in allKeys)
{
    Debug.Log($"{key}: {StorageService.GetString(key)}");
}
```

### Check current storage
```csharp
string type = StorageService.GetCurrentStorageType();
Debug.Log($"Using: {type}");

var storage = StorageService.GetCurrentStorage();
if (storage is JsonStorage json)
{
    Debug.Log($"IsDirty: {json.IsDirty}");
    Debug.Log($"AutoSave: {json.IsAutoSaveEnabled}");
}
```

### Manual save (JsonStorage only)
```csharp
StorageService.SetInt("score", 100);
StorageService.Save(); // Force save
```

## Troubleshooting

### Q: Migration không hoạt động?
A: Check logs để xem error. Thường do:
- Không có quyền ghi file
- Encryption key sai
- Storage chưa có data

### Q: Data bị mất sau migration?
A: Không thể xảy ra! Hệ thống có backup. Check file `migration_backup.json`

### Q: GetAllKeys() trả về empty?
A: LocalStorage trong builds không support enumerate keys. Dùng JsonStorage thay thế.

### Q: Performance kém khi save nhiều?
A: Dùng JsonStorage với `autoSave: false` và `Save()` manual.
