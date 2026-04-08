using System.Collections.Generic;
using Firebase.Analytics;

namespace DucMinh
{
    public class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        private bool _initialized;
        public bool Initialized => _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            _initialized = true;
        }

        public void SetUserId(string userId)
        {
            FirebaseAnalytics.SetUserId(userId);
        }

        public void LogEvent(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters = null)
        {
            if (parameters is not { Count: > 0 })
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = new Parameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                firebaseParams[i] = CreateParameter(parameters[i]);
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }

        public void SetUserProperty(string key, object value)
        {
            FirebaseAnalytics.SetUserProperty(key, value?.ToString());
        }

        private static Parameter CreateParameter(AnalyticsEventParameter parameter)
        {
            return parameter.Type switch
            {
                AnalyticsEventParameterType.String => new Parameter(parameter.Key, parameter.StringValue),
                AnalyticsEventParameterType.Long => new Parameter(parameter.Key, parameter.LongValue),
                AnalyticsEventParameterType.Double => new Parameter(parameter.Key, parameter.DoubleValue),
                AnalyticsEventParameterType.Dictionary => new Parameter(parameter.Key, parameter.DictionaryValue),
                AnalyticsEventParameterType.DictionaryList => new Parameter(parameter.Key, parameter.DictionaryListValue),
                _ => new Parameter(parameter.Key, string.Empty)
            };
        }
    }
}
