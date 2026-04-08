namespace DucMinh.GenerateScript
{
    /// <summary>
    /// Chính sách xử lý khi file đã tồn tại.
    /// </summary>
    public enum OverwriteMode
    {
        /// <summary>Bỏ qua, không ghi đè.</summary>
        Skip,

        /// <summary>Ghi đè không hỏi.</summary>
        Overwrite,

        /// <summary>Hiện dialog tổng hợp tất cả conflicts, user tự chọn.</summary>
        Ask
    }
}
