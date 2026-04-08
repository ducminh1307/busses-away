# Persistent Storage Migration - Workflow

## 🎯 Vấn đề đã giải quyết: "Nếu đã migrate rồi thì lần sau xử lý kiểu gì?"

### **Giải pháp: Persistent Storage Configuration**

Hệ thống tự động lưu storage type vào **PlayerPrefs** (persistent across app restarts) để:
- ✅ Nhớ storage type đã migrate
- ✅ Tự động load đúng storage khi app khởi động lại
- ✅ **KHÔNG CẦN** migrate lại mỗi lần start app

---

## 📋 Workflow Timeline

### **Lần 1: App chạy lần đầu**
```csharp
void Awake()
{
    StorageService.Init();
}
```

**Điều gì xảy ra:**
1. Kiểm tra PlayerPrefs → Không có config
2. Load **LocalStorage** (default)
3. User chơi game, data lưu vào PlayerPrefs

**PlayerPrefs Config:** ❌ Chưa có

---

### **Lần 2: Update app - Thực hiện migration**
```csharp
void Awake()
{
    StorageService.Init(); // Load LocalStorage từ last session
    
    // App update, cần migrate sang JsonStorage
    bool success = StorageService.MigrateTo(
        StorageMigrator.StorageType.JsonStorage,
        fileName: "game_save.json"
    );
}
```

**Điều gì xảy ra:**
1. `Init()` load từ config → LocalStorage
2. `MigrateTo()` được gọi
3. Migrate all data từ LocalStorage → JsonStorage
4. **Tự động save config vào PlayerPrefs:**
   - `PREF_STORAGE_TYPE = "JsonStorage"`
   - `PREF_STORAGE_FILE = "game_save.json"`
5. App giờ dùng JsonStorage

**PlayerPrefs Config:** ✅ `StorageType = JsonStorage, File = game_save.json`

---

### **Lần 3+: User khởi động app lại**
```csharp
void Awake()
{
    StorageService.Init(); // ← CHỈ CẦN DÒNG NÀY!
}
```

**Điều gì xảy ra:**
1. `Init()` kiểm tra PlayerPrefs
2. Thấy config: `StorageType = JsonStorage, File = game_save.json`
3. **Tự động load JsonStorage** với đúng file name
4. **KHÔNG migrate lại!**
5. Load data từ `game_save.json`
6. User tiếp tục chơi

**PlayerPrefs Config:** ✅ `StorageType = JsonStorage, File = game_save.json`

---

## 🔄 Flow Chart

```
[App Start]
    ↓
[StorageService.Init()]
    ↓
[Check PlayerPrefs cho saved config]
    ↓
    ├─ Có config → Load đúng storage type từ config
    │                    ↓
    │              [Use that storage]
    │
    └─ Không có config → Load LocalStorage (default)
                         ↓
                   [Use LocalStorage]

[Nếu muốn migrate]
    ↓
[Call MigrateTo(newType)]
    ↓
[Migrate data]
    ↓
[Save config to PlayerPrefs]
    ↓
[Lần sau app start sẽ tự động load newType]
```

---

## 💡 Key Methods

### **Tự động load từ config**
```csharp
StorageService.Init();
```
- Không cần parameter
- Tự động load từ saved config
- Lần đầu sẽ dùng LocalStorage

### **Check config hiện tại**
```csharp
var savedType = StorageMigrator.GetSavedStorageType();
// Returns: LocalStorage, JsonStorage, EncryptionStorage, or null
```

### **Migrate và save config**
```csharp
StorageService.MigrateTo(
    StorageMigrator.StorageType.JsonStorage,
    fileName: "save.json"
);
```
- Migrate data
- **Tự động save config**
- Lần sau auto-load

### **Clear config (testing)**
```csharp
StorageMigrator.ClearStorageConfig();
```

---

## 📝 Best Practice: Version-Based Migration

```csharp
void Awake()
{
    // Auto-load từ saved config
    StorageService.Init();
    
    // Check app version
    int savedVersion = StorageService.GetInt("_app_version", 0);
    int currentVersion = 2; // Your app version
    
    if (savedVersion < currentVersion)
    {
        PerformMigrationForVersion(savedVersion, currentVersion);
    }
}

void PerformMigrationForVersion(int from, int to)
{
    // Version 0 → 1: Migrate to JsonStorage
    if (from < 1)
    {
        StorageService.MigrateTo(
            StorageMigrator.StorageType.JsonStorage,
            "v1_data.json"
        );
        StorageService.SetInt("_app_version", 1);
    }
    
    // Version 1 → 2: Add encryption
    if (from < 2)
    {
        StorageService.MigrateTo(
            StorageMigrator.StorageType.EncryptionStorage,
            "v2_encrypted.json",
            SystemInfo.deviceUniqueIdentifier
        );
        StorageService.SetInt("_app_version", 2);
    }
}
```

**Lợi ích:**
- Chỉ migrate khi cần (version thấp hơn)
- Mỗi lần start chỉ check version
- Không migrate lại nếu đã update

---

## 🎮 Example: Typical Game Lifecycle

### **Day 1: Soft Launch**
```csharp
void Start()
{
    StorageService.Init(); // LocalStorage
}
```
- Config: None → LocalStorage

### **Day 30: Add Cloud Save Feature**
```csharp
void Start()
{
    StorageService.Init(); // Auto-loads LocalStorage
    
    if (cloudSaveEnabled)
    {
        StorageService.MigrateTo(
            StorageMigrator.StorageType.JsonStorage,
            "cloud_sync_data.json"
        );
    }
}
```
- Config saved: JsonStorage + file name

### **Day 31-100: Normal Operations**
```csharp
void Start()
{
    StorageService.Init(); // Auto-loads JsonStorage
    // No migration needed!
}
```
- Auto-loads from config
- Everything works seamlessly

### **Day 101: Add Anti-Cheat Encryption**
```csharp
void Start()
{
    StorageService.Init(); // Auto-loads JsonStorage
    
    if (needsEncryption)
    {
        StorageService.MigrateTo(
            StorageMigrator.StorageType.EncryptionStorage,
            "encrypted_data.json",
            GetEncryptionKey()
        );
    }
}
```
- Migrates from JsonStorage → EncryptionStorage
- Config updated
- Future launches auto-load EncryptionStorage

---

## ⚠️ Important Notes

1. **Config lưu ở đâu?**
   - PlayerPrefs (persistent)
   - Keys: `DucMinh_StorageType`, `DucMinh_StorageFile`, `DucMinh_EncryptionKey`

2. **Khi nào config được save?**
   - Khi `MigrateTo()` thành công
   - Tự động, không cần gọi Save()

3. **Khi nào config được load?**
   - Khi `StorageService.Init()` được gọi
   - Khi access `StorageService._dataStorage` lần đầu
   - Tự động, transparent

4. **Có thể migrate nhiều lần?**
   - Có! LocalStorage → JsonStorage → EncryptionStorage
   - Mỗi lần migrate, config được update
   - Lần sau app start sẽ load storage mới nhất

5. **Testing/Development**
   ```csharp
   // Clear config để test lại migration
   StorageMigrator.ClearStorageConfig();
   ```

---

## ✅ Summary

**Trước khi có persistent config:**
- Mỗi lần app start phải migrate lại
- Lãng phí thời gian và resources
- Risk of data corruption

**Sau khi có persistent config:**
- Migrate 1 lần duy nhất
- Lần sau app tự động load đúng storage
- Fast, safe, transparent
- **User không nhận biết được!**

**Code của bạn:**
```csharp
void Awake()
{
    StorageService.Init(); // That's it!
}
```

Hệ thống tự động lo tất cả! 🎉
