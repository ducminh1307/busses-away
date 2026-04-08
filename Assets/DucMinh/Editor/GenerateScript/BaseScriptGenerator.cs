using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DucMinh.GenerateScript
{
    public abstract class BaseScriptGenerator
    {
        // ─── Phase 1: Identity ───────────────────────────────────────────────────

        /// <summary>Tên hiển thị trong sidebar.</summary>
        public abstract string Name { get; }

        // ─── Phase 3: Metadata ───────────────────────────────────────────────────

        /// <summary>ID duy nhất, dùng cho preset storage và discovery.</summary>
        public virtual string Id => GetType().Name;

        /// <summary>Nhóm hiển thị trong sidebar (vd: "Scripts", "UI", "Data").</summary>
        public virtual string Category => "General";

        /// <summary>Mô tả ngắn về generator này.</summary>
        public virtual string Description => string.Empty;

        // ─── Phase 1: UI Setup ───────────────────────────────────────────────────

        /// <summary>Build UI settings trong right panel.</summary>
        public abstract void SetupGUI(VisualElement container);

        // ─── Phase 1: Validation & Preview ──────────────────────────────────────

        /// <summary>
        /// Validate cấu hình hiện tại.
        /// Trả về ValidationResult với errors/warnings.
        /// </summary>
        public virtual ValidationResult Validate()
        {
            return ValidationResult.Ok();
        }

        /// <summary>
        /// Trả danh sách files/folders sẽ được tạo (để preview).
        /// </summary>
        public virtual List<PreviewItem> BuildPreview()
        {
            return new List<PreviewItem>();
        }

        // ─── Phase 2: Generate ───────────────────────────────────────────────────

        /// <summary>
        /// Thực hiện generation. Trả về GenerationResult.
        /// </summary>
        public abstract GenerationResult Generate(OverwriteMode overwriteMode = OverwriteMode.Skip);

        // ─── Phase 6: Batch Plan ─────────────────────────────────────────────────

        /// <summary>
        /// Build GenerationPlan cho batch mode. Mặc định trả null (không hỗ trợ batch).
        /// Override để hỗ trợ batch generation.
        /// </summary>
        public virtual GenerationPlan BuildPlan() => null;

        // ─── Phase 3: Preset ─────────────────────────────────────────────────────

        private static readonly string PresetFilePath = Path.Combine(
            Application.dataPath, "../ProjectSettings/GeneratorPresets.json");

        private static EditorJsonStorage _presetStorage;

        private static EditorJsonStorage PresetStorage =>
            _presetStorage ??= new EditorJsonStorage(PresetFilePath);

        /// <summary>
        /// Lưu preset hiện tại dưới dạng JSON.
        /// Subclass override CollectPresetData() để cung cấp data.
        /// </summary>
        public void SavePreset(string presetName)
        {
            var data = CollectPresetData();
            if (data == null) return;

            var json = JsonConvert.SerializeObject(data);
            PresetStorage.SetString($"{Id}:{presetName}", json);
            Debug.Log($"[{Name}] Preset '{presetName}' saved.");
        }

        /// <summary>
        /// Load preset theo tên. Subclass override ApplyPresetData() để áp dụng.
        /// </summary>
        public void LoadPreset(string presetName)
        {
            var json = PresetStorage.GetString($"{Id}:{presetName}");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[{Name}] Preset '{presetName}' not found.");
                return;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (data != null) ApplyPresetData(data);
        }

        /// <summary>Xóa preset.</summary>
        public void DeletePreset(string presetName)
        {
            PresetStorage.Delete($"{Id}:{presetName}");
        }

        /// <summary>Lấy danh sách tên preset của generator này.</summary>
        public List<string> GetPresetNames()
        {
            var prefix = $"{Id}:";
            var names = new List<string>();
            foreach (var key in PresetStorage.GetAllKeys())
            {
                if (key.StartsWith(prefix))
                    names.Add(key.Substring(prefix.Length));
            }
            return names;
        }

        /// <summary>Override để cung cấp data khi SavePreset.</summary>
        protected virtual Dictionary<string, string> CollectPresetData() => null;

        /// <summary>Override để áp dụng data khi LoadPreset.</summary>
        protected virtual void ApplyPresetData(Dictionary<string, string> data) { }

        // ─── Phase 5: Shared Settings ────────────────────────────────────────────

        /// <summary>Shared settings singleton.</summary>
        protected static GeneratorSettings Settings => GeneratorSettings.Instance;

        // ─── File Helpers (Phase 2: OverwriteMode aware) ─────────────────────────

        /// <summary>
        /// Ghi file với overwrite policy.
        /// Trả về trạng thái: Created, Skipped, Overwritten.
        /// </summary>
        protected string CreateFile(string relativeAssetsPath, string content,
            OverwriteMode overwriteMode, GenerationResult result)
        {
            string absolutePath = Path.Combine(
                Application.dataPath, relativeAssetsPath.Replace("Assets/", ""));

            // Đảm bảo thư mục tồn tại
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            bool fileExists = File.Exists(absolutePath);

            if (fileExists && overwriteMode == OverwriteMode.Skip)
            {
                result?.AddSkipped(relativeAssetsPath);
                Debug.Log($"[GenerateScript] Skipped (exists): {relativeAssetsPath}");
                return "skipped";
            }

            try
            {
                File.WriteAllText(absolutePath, content);

                if (fileExists)
                {
                    result?.AddOverwritten(relativeAssetsPath);
                    Debug.Log($"[GenerateScript] Overwritten: {relativeAssetsPath}");
                    return "overwritten";
                }
                else
                {
                    result?.AddCreated(relativeAssetsPath);
                    Debug.Log($"[GenerateScript] Created: {relativeAssetsPath}");
                    return "created";
                }
            }
            catch (Exception e)
            {
                result?.AddFailed(relativeAssetsPath);
                Debug.LogError($"[GenerateScript] Failed to write {relativeAssetsPath}: {e.Message}");
                return "failed";
            }
        }

        /// <summary>
        /// Legacy overload — ghi đè không hỏi (tương thích ngược).
        /// </summary>
        protected void CreateFile(string relativeAssetsPath, string content)
        {
            CreateFile(relativeAssetsPath, content, OverwriteMode.Overwrite, null);
        }

        /// <summary>
        /// Detect file nào đã tồn tại trong preview list (dùng cho Ask mode dry-run).
        /// </summary>
        public List<string> DetectConflicts(List<PreviewItem> preview)
        {
            var conflicts = new List<string>();
            foreach (var item in preview)
            {
                if (item.Type == PreviewItemType.File)
                {
                    var abs = Path.Combine(Application.dataPath,
                        item.Path.Replace("Assets/", ""));
                    if (File.Exists(abs)) conflicts.Add(item.Path);
                }
            }
            return conflicts;
        }

        /// <summary>Tạo folder (giữ nguyên behavior).</summary>
        protected void CreateFolder(string relativeAssetsPath)
        {
            var absolutePath = Path.Combine(Application.dataPath,
                relativeAssetsPath.Replace("Assets/", ""));

            if (Directory.Exists(absolutePath)) return;
            Directory.CreateDirectory(absolutePath);
            Debug.Log($"[GenerateScript] Created folder: {relativeAssetsPath}");
        }

        // ─── Phase 6: Plan Execution ─────────────────────────────────────────────

        /// <summary>
        /// Thực thi GenerationPlan. Gọi từ GenerateScript EditorWindow ở batch mode.
        /// </summary>
        protected GenerationResult ExecutePlan(GenerationPlan plan,
            OverwriteMode overwriteMode = OverwriteMode.Skip)
        {
            var result = new GenerationResult();

            foreach (var folder in plan.Folders)
            {
                var abs = folder.AbsolutePath;
                if (!Directory.Exists(abs))
                {
                    Directory.CreateDirectory(abs);
                    result.AddCreated(folder.RelativeAssetsPath);
                }
            }

            foreach (var file in plan.Files)
            {
                CreateFile(file.RelativeAssetsPath, file.Content, overwriteMode, result);
            }

            foreach (var hook in plan.PostHooks)
            {
                try { hook?.Invoke(); }
                catch (Exception e) { Debug.LogError($"[GenerateScript] PostHook failed: {e.Message}"); }
            }

            return result;
        }

        // ─── Asset Database ──────────────────────────────────────────────────────

        /// <summary>Refresh AssetDatabase sau khi generate xong.</summary>
        protected void RefreshAssetDatabase()
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}