#if LOCALIZATION
namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        private const string DEFAULT_TABLE = "InGame";

        public static string GetTextWithID(this string key)
        {
            return LocalizationHelper.GetTextWithID(DEFAULT_TABLE, key);
        }

        public static string GetTextWithID(this string key, params object[] args)
        {
            return LocalizationHelper.GetTextWithID(DEFAULT_TABLE, key, args);
        }
    }
}
#endif