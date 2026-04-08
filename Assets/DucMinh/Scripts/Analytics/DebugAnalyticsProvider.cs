using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DucMinh
{
    public class DebugAnalyticsProvider : IAnalyticsProvider
    {
        private const string StorageFileName = "analytics.json";
        private const string LogKeyPrefix = "Log_";
        private const string UserIdKey = "UserId";
        private const string LogCountKey = "LogCount";
        private const string UserPropertyPrefix = "UserProperty_";
        private const int LogSchemaVersion = 1;

        private bool _initialized;
        private IDataStorage _storage;
        private int _logCount;

        public bool Initialized => _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            _storage = new JsonStorage(StorageFileName);
            _logCount = Math.Max(_storage.GetInt(LogCountKey, 0), GetLatestLogCount());
            _initialized = true;
        }

        public void SetUserId(string userId)
        {
            _storage.SetString(UserIdKey, userId);

            var message = $"[Analytics][Debug] UserId: {userId}";
            AppendLog(new DebugAnalyticsLogEntry
            {
                Category = "SetUserId",
                Message = message,
                UserId = userId
            });
        }

        public void LogEvent(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters = null)
        {
            var message = BuildEventLog(eventName, parameters);
            Log.Debug(message);
            AppendLog(new DebugAnalyticsLogEntry
            {
                Category = "Event",
                Message = message,
                EventName = eventName,
                Parameters = CreateSerializableParameters(parameters)
            });
        }

        public void SetUserProperty(string key, object value)
        {
            var valueText = value?.ToString() ?? string.Empty;
            _storage.SetString($"{UserPropertyPrefix}{key}", valueText);

            var message = $"[Analytics][Debug] Set User Property: '{key}' = '{valueText}'";
            Log.Debug(message);
            AppendLog(new DebugAnalyticsLogEntry
            {
                Category = "SetUserProperty",
                Message = message,
                PropertyKey = key,
                PropertyValue = valueText,
                PropertyValueType = value?.GetType().Name
            });
        }

        private string BuildEventLog(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters)
        {
            var sb = new StringBuilder();
            sb.Append($"[Analytics][Debug] Event: {eventName}");
            if (parameters is { Count: > 0 })
            {
                sb.Append(" | Parameters: [");
                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    sb.Append($"{parameter.Key}: {parameter.GetDisplayValue()}");
                    if (i < parameters.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append(']');
            }
            else
            {
                sb.Append(" | No parameters.");
            }

            return sb.ToString();
        }

        private List<DebugAnalyticsLogParameter> CreateSerializableParameters(IReadOnlyList<AnalyticsEventParameter> parameters)
        {
            var serializedParameters = new List<DebugAnalyticsLogParameter>();
            if (parameters == null)
            {
                return serializedParameters;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];
                serializedParameters.Add(new DebugAnalyticsLogParameter
                {
                    Key = parameter.Key,
                    Type = parameter.Type.ToString(),
                    Value = GetParameterValue(parameter)
                });
            }

            return serializedParameters;
        }

        private object GetParameterValue(AnalyticsEventParameter parameter)
        {
            return parameter.Type switch
            {
                AnalyticsEventParameterType.String => parameter.StringValue,
                AnalyticsEventParameterType.Long => parameter.LongValue,
                AnalyticsEventParameterType.Double => parameter.DoubleValue,
                AnalyticsEventParameterType.Dictionary => parameter.DictionaryValue,
                AnalyticsEventParameterType.DictionaryList => parameter.DictionaryListValue,
                _ => null
            };
        }

        private int GetLatestLogCount()
        {
            var allKeys = _storage.GetAllKeys();
            var latestLogCount = 0;

            for (int i = 0; i < allKeys.Count; i++)
            {
                var key = allKeys[i];
                if (!key.StartsWith(LogKeyPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                var logIndexText = key.Substring(LogKeyPrefix.Length);
                if (int.TryParse(logIndexText, out var logIndex) && logIndex > latestLogCount)
                {
                    latestLogCount = logIndex;
                }
            }

            return latestLogCount;
        }

        private void AppendLog(DebugAnalyticsLogEntry entry)
        {
            _logCount++;

            entry.SchemaVersion = LogSchemaVersion;
            entry.Timestamp = DateTime.Now.ToString("O");

            var key = $"{LogKeyPrefix}{_logCount:D6}";
            _storage.SetString(key, JsonConvert.SerializeObject(entry));
            _storage.SetInt(LogCountKey, _logCount);
        }
    }

    public sealed class DebugAnalyticsLogEntry
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("eventName", NullValueHandling = NullValueHandling.Ignore)]
        public string EventName { get; set; }

        [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        public List<DebugAnalyticsLogParameter> Parameters { get; set; }

        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty("propertyKey", NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyKey { get; set; }

        [JsonProperty("propertyValue", NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyValue { get; set; }

        [JsonProperty("propertyValueType", NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyValueType { get; set; }
    }

    public sealed class DebugAnalyticsLogParameter
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
