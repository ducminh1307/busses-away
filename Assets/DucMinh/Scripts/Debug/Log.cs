using System.Diagnostics;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace DucMinh
{
    public static class Log
    {
        private const string NullMessage = "<color=#f88>\u26a0 [ERROR] Log message is null</color>";
        
        private const string DebugPrefix = "[DEBUG] \u27a4 ";
        private const string WarningPrefix = "[WARNING] \u27a4 ";
        private const string ErrorPrefix = "[ERROR] \u27a4 ";

        #region Helpers

        private static string FormatMessage(string prefix, object message, string hexColor)
        {
            if (message == null) message = NullMessage;
            
            if (!string.IsNullOrEmpty(hexColor) && hexColor.IsHexColor())
            {
                return $"<color={hexColor}>{prefix}{message}</color>";
            }
            
            return $"{prefix}{message}";
        }
        
        private static string FormatMessage(string prefix, object message, Color color)
        {
            if (message == null) message = NullMessage;
            return $"<color={color.ToHex()}>{prefix}{message}</color>";
        }

        #endregion

        #region Debug

        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(object message)
        {
            UnityDebug.Log($"{DebugPrefix}{message ?? NullMessage}");
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(object message, string color)
        {
            UnityDebug.Log(FormatMessage(DebugPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(object message, Color color)
        {
            UnityDebug.Log(FormatMessage(DebugPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(string format, params object[] args)
        {
            UnityDebug.LogFormat($"{DebugPrefix}{(format ?? NullMessage)}", args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(string format, string color, params object[] args)
        {
            UnityDebug.LogFormat(FormatMessage(DebugPrefix, format, color), args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Debug(string format, Color color, params object[] args)
        {
            UnityDebug.LogFormat(FormatMessage(DebugPrefix, format, color), args);
        }

        #endregion
        
        #region Warning
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(object message)
        {
            UnityDebug.LogWarning($"{WarningPrefix}{message ?? NullMessage}");
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(object message, string color)
        {
            UnityDebug.LogWarning(FormatMessage(WarningPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(object message, Color color)
        {
            UnityDebug.LogWarning(FormatMessage(WarningPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(string format, params object[] args)
        {
            UnityDebug.LogWarningFormat($"{WarningPrefix}{(format ?? NullMessage)}", args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(string format, string color, params object[] args)
        {
            UnityDebug.LogWarningFormat(FormatMessage(WarningPrefix, format, color), args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Warning(string format, Color color, params object[] args)
        {
            UnityDebug.LogWarningFormat(FormatMessage(WarningPrefix, format, color), args);
        }
        
        #endregion

        #region Error

        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(object message)
        {
            UnityDebug.LogError($"{ErrorPrefix}{message ?? NullMessage}");
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(object message, string color)
        {
            UnityDebug.LogError(FormatMessage(ErrorPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(object message, Color color)
        {
            UnityDebug.LogError(FormatMessage(ErrorPrefix, message, color));
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(string format, params object[] args)
        {
            UnityDebug.LogErrorFormat($"{ErrorPrefix}{(format ?? NullMessage)}", args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(string format, string color, params object[] args)
        {
            UnityDebug.LogErrorFormat(FormatMessage(ErrorPrefix, format, color), args);
        }
        
        [Conditional("DEBUG"), Conditional("DEBUG_MOD")]
        public static void Error(string format, Color color, params object[] args)
        {
            UnityDebug.LogErrorFormat(FormatMessage(ErrorPrefix, format, color), args);
        }

        #endregion
    }
}
