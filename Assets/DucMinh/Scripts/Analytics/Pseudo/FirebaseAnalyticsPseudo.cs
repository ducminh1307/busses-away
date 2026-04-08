#if !USE_FIREBASE_ANALYTICS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DucMinh;

namespace Firebase.Analytics
{
    public enum ConsentStatus
    {
        Denied = 0,
        Granted = 1,
    }

    public enum ConsentType
    {
        AdStorage = 0,
        AnalyticsStorage = 1,
        AdUserData = 2,
        AdPersonalization = 3,
    }

    public readonly struct Parameter
    {
        public string Name { get; }
        public object Value { get; }

        public Parameter(string parameterName, string parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }

        public Parameter(string parameterName, long parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }

        public Parameter(string parameterName, double parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }

        public Parameter(string parameterName, IDictionary<string, object> parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }

        public Parameter(string parameterName, IEnumerable<IDictionary<string, object>> parameterValue)
        {
            Name = parameterName;
            Value = parameterValue;
        }
    }

    public static class FirebaseAnalytics
    {
        private static bool _collectionEnabled = true;
        private static string _userId;
        private static string _analyticsInstanceId = Guid.NewGuid().ToString("N");
        private static long _sessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private static TimeSpan _sessionTimeout = TimeSpan.FromMinutes(30);

        private static readonly Dictionary<ConsentType, ConsentStatus> _consentSettings = new();
        private static readonly List<Parameter> _defaultEventParameters = new();

        public static Task<string> GetAnalyticsInstanceIdAsync()
        {
            return Task.FromResult(_analyticsInstanceId);
        }

        public static Task<long> GetSessionIdAsync()
        {
            return Task.FromResult(_sessionId);
        }

        public static void InitiateOnDeviceConversionMeasurementWithEmailAddress(string emailAddress)
        {
            Log.Debug($"[Firebase Analytics - PSEUDO] On-device conversion email: '{emailAddress}'");
        }

        public static void InitiateOnDeviceConversionMeasurementWithHashedEmailAddress(byte[] hashedEmailAddress)
        {
            Log.Debug($"[Firebase Analytics - PSEUDO] On-device conversion hashed email: {FormatBytes(hashedEmailAddress)}");
        }

        public static void InitiateOnDeviceConversionMeasurementWithHashedPhoneNumber(byte[] hashedPhoneNumber)
        {
            Log.Debug($"[Firebase Analytics - PSEUDO] On-device conversion hashed phone: {FormatBytes(hashedPhoneNumber)}");
        }

        public static void InitiateOnDeviceConversionMeasurementWithPhoneNumber(string phoneNumber)
        {
            Log.Debug($"[Firebase Analytics - PSEUDO] On-device conversion phone: '{phoneNumber}'");
        }

        public static void LogEvent(string name, string parameterName, string parameterValue)
        {
            LogEvent(name, new Parameter(parameterName, parameterValue));
        }

        public static void LogEvent(string name, string parameterName, double parameterValue)
        {
            LogEvent(name, new Parameter(parameterName, parameterValue));
        }

        public static void LogEvent(string name, string parameterName, long parameterValue)
        {
            LogEvent(name, new Parameter(parameterName, parameterValue));
        }

        public static void LogEvent(string name, string parameterName, int parameterValue)
        {
            LogEvent(name, new Parameter(parameterName, (long)parameterValue));
        }

        public static void LogEvent(string name)
        {
            LogEvent(name, Array.Empty<Parameter>());
        }

        public static void LogEvent(string name, params Parameter[] parameters)
        {
            LogEvent(name, (IEnumerable<Parameter>)parameters);
        }

        public static void LogEvent(string name, IEnumerable<Parameter> parameters)
        {
            var mergedParameters = MergeDefaultParameters(parameters);
            Log.Debug(BuildEventLog(name, mergedParameters));
        }

        public static void ResetAnalyticsData()
        {
            _userId = null;
            _analyticsInstanceId = Guid.NewGuid().ToString("N");
            _sessionId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _consentSettings.Clear();
            _defaultEventParameters.Clear();

            Log.Debug("[Firebase Analytics - PSEUDO] Analytics data reset.");
        }

        public static void SetAnalyticsCollectionEnabled(bool enabled)
        {
            _collectionEnabled = enabled;
            Log.Debug($"[Firebase Analytics - PSEUDO] Collection enabled: {enabled}");
        }

        public static void SetConsent(IDictionary<ConsentType, ConsentStatus> consentSettings)
        {
            _consentSettings.Clear();
            if (consentSettings != null)
            {
                foreach (var pair in consentSettings)
                {
                    _consentSettings[pair.Key] = pair.Value;
                }
            }

            Log.Debug($"[Firebase Analytics - PSEUDO] Consent updated: {FormatConsentSettings(_consentSettings)}");
        }

        public static void SetDefaultEventParameters(params Parameter[] parameters)
        {
            SetDefaultEventParameters((IEnumerable<Parameter>)parameters);
        }

        public static void SetDefaultEventParameters(IEnumerable<Parameter> parameters)
        {
            _defaultEventParameters.Clear();
            if (parameters != null)
            {
                _defaultEventParameters.AddRange(parameters);
            }

            Log.Debug($"[Firebase Analytics - PSEUDO] Default event parameters: {FormatParameters(_defaultEventParameters)}");
        }

        public static void SetSessionTimeoutDuration(TimeSpan timeSpan)
        {
            _sessionTimeout = timeSpan;
            Log.Debug($"[Firebase Analytics - PSEUDO] Session timeout: {_sessionTimeout}");
        }

        public static void SetUserId(string userId)
        {
            _userId = userId;
            Log.Debug($"[Firebase Analytics - PSEUDO] Set UserId: '{userId}'");
        }

        public static void SetUserProperty(string name, string property)
        {
            Log.Debug($"[Firebase Analytics - PSEUDO] Set User Property: '{name}' = '{property}'");
        }

        private static IEnumerable<Parameter> MergeDefaultParameters(IEnumerable<Parameter> parameters)
        {
            var eventParameters = parameters?.ToList() ?? new List<Parameter>();
            if (_defaultEventParameters.Count == 0)
            {
                return eventParameters;
            }

            var eventParameterNames = new HashSet<string>(eventParameters.Select(parameter => parameter.Name));
            var mergedParameters = new List<Parameter>(_defaultEventParameters.Count + eventParameters.Count);
            foreach (var defaultParameter in _defaultEventParameters)
            {
                if (eventParameterNames.Contains(defaultParameter.Name)) continue;
                mergedParameters.Add(defaultParameter);
            }

            mergedParameters.AddRange(eventParameters);
            return mergedParameters;
        }

        private static string BuildEventLog(string name, IEnumerable<Parameter> parameters)
        {
            var sb = new StringBuilder();
            sb.Append($"[Firebase Analytics - PSEUDO] Event: {name}");
            sb.Append($" | CollectionEnabled: {_collectionEnabled}");

            if (!string.IsNullOrEmpty(_userId))
            {
                sb.Append($" | UserId: {_userId}");
            }

            var parameterList = parameters?.ToList() ?? new List<Parameter>();
            if (parameterList.Count > 0)
            {
                sb.Append(" | Params: ");
                sb.Append(FormatParameters(parameterList));
            }

            return sb.ToString();
        }

        private static string FormatParameters(IEnumerable<Parameter> parameters)
        {
            var list = parameters?.ToList() ?? new List<Parameter>();
            if (list.Count == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < list.Count; i++)
            {
                var parameter = list[i];
                sb.Append($"{parameter.Name}: {FormatValue(parameter.Value)}");
                if (i < list.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatConsentSettings(IDictionary<ConsentType, ConsentStatus> consentSettings)
        {
            if (consentSettings == null || consentSettings.Count == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');
            int index = 0;
            foreach (var pair in consentSettings)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (index < consentSettings.Count - 1)
                {
                    sb.Append(", ");
                }
                index++;
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => "null",
                IDictionary<string, object> dict => FormatDictionary(dict),
                IEnumerable<IDictionary<string, object>> dictList => FormatDictionaryList(dictList),
                _ => value.ToString()
            };
        }

        private static string FormatDictionary(IDictionary<string, object> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0) return "{}";

            var sb = new StringBuilder();
            sb.Append('{');
            int index = 0;
            foreach (var pair in dictionary)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (index < dictionary.Count - 1)
                {
                    sb.Append(", ");
                }
                index++;
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static string FormatDictionaryList(IEnumerable<IDictionary<string, object>> dictionaries)
        {
            if (dictionaries == null) return "[]";

            var list = dictionaries.ToList();
            if (list.Count == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(FormatDictionary(list[i]));
                if (i < list.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(']');
            return sb.ToString();
        }

        private static string FormatBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "[]";
            return $"[{BitConverter.ToString(bytes)}]";
        }
    }
}
#endif
