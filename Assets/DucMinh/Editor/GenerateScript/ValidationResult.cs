using System.Collections.Generic;

namespace DucMinh.GenerateScript
{
    /// <summary>
    /// Kết quả validation của một generator trước khi generate.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();

        public void AddError(string message) => Errors.Add(message);
        public void AddWarning(string message) => Warnings.Add(message);

        public static ValidationResult Ok() => new ValidationResult();

        public static ValidationResult Fail(string error)
        {
            var result = new ValidationResult();
            result.AddError(error);
            return result;
        }
    }
}
