namespace DucMinh
{
    public static class Assert
    {
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                ThrowAssert("Assertion Failed: " + message);
            }
        }

        public static void IsFalse(bool condition, string message)
        {
            if (condition)
            {
                ThrowAssert("Assertion Failed: " + message);
            }
        }

        public static void IsNull(object obj, string message)
        {
            if (obj != null)
            {
                ThrowAssert("Assertion Failed: " + message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG_MODE")]
        private static void ThrowAssert(string message)
        {
            Log.Debug($"Assertion: {message}");
        }
    }
}