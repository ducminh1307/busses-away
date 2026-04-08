using System.Collections.Generic;

namespace DucMinh
{
    public interface IAnalyticsProvider
    {
        bool Initialized { get; }
        void Initialize();
        void SetUserId(string userId);
        void SetUserProperty(string key, object value);
        void LogEvent(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters = null);
    }
}
