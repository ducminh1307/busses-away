using System.Collections.Generic;

namespace DucMinh.GenerateScript
{
    /// <summary>
    /// Kết quả sau khi một generator chạy xong.
    /// </summary>
    public class GenerationResult
    {
        public List<string> Created { get; } = new List<string>();
        public List<string> Skipped { get; } = new List<string>();
        public List<string> Overwritten { get; } = new List<string>();
        public List<string> Failed { get; } = new List<string>();

        public bool HasAnyResult => Created.Count > 0 || Skipped.Count > 0 || Overwritten.Count > 0 || Failed.Count > 0;

        public void AddCreated(string path) => Created.Add(path);
        public void AddSkipped(string path) => Skipped.Add(path);
        public void AddOverwritten(string path) => Overwritten.Add(path);
        public void AddFailed(string path) => Failed.Add(path);

        public string Summary =>
            $"✅ Created: {Created.Count}  " +
            $"⏭ Skipped: {Skipped.Count}  " +
            $"✏️ Overwritten: {Overwritten.Count}  " +
            $"❌ Failed: {Failed.Count}";
    }
}
