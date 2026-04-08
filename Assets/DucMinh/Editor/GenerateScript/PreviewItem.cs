namespace DucMinh.GenerateScript
{
    public enum PreviewItemType
    {
        File,
        Folder
    }

    /// <summary>
    /// Mỗi item mà generator sẽ tạo ra (file hoặc folder).
    /// </summary>
    public class PreviewItem
    {
        public string Path { get; }
        public PreviewItemType Type { get; }

        public PreviewItem(string path, PreviewItemType type)
        {
            Path = path;
            Type = type;
        }

        public static PreviewItem File(string path) => new PreviewItem(path, PreviewItemType.File);
        public static PreviewItem Folder(string path) => new PreviewItem(path, PreviewItemType.Folder);

        public override string ToString() => $"[{Type}] {Path}";
    }
}
