using System;
using System.Collections.Generic;

namespace DucMinh
{
    public readonly struct AnalyticsEventBuilder : IDisposable
    {
        private readonly string _eventName;
        private readonly List<AnalyticsEventParameter> _parameters;
        private readonly DelegatedPool<List<AnalyticsEventParameter>> _pool;

        public AnalyticsEventBuilder(string eventName, List<AnalyticsEventParameter> parameters, DelegatedPool<List<AnalyticsEventParameter>> pool)
        {
            _eventName = eventName;
            _parameters = parameters;
            _pool = pool;
        }

        public AnalyticsEventBuilder Add(string key, object value)
        {
            ValidateDuplicateKey(key);
            _parameters.Add(AnalyticsEventParameter.FromObject(key, value));
            return this;
        }

        public AnalyticsEventBuilder Add(string key, long value)
        {
            ValidateDuplicateKey(key);
            _parameters.Add(new AnalyticsEventParameter(key, value));
            return this;
        }

        public AnalyticsEventBuilder Add(string key, int value)
        {
            return Add(key, (long)value);
        }

        public AnalyticsEventBuilder Add(string key, double value)
        {
            ValidateDuplicateKey(key);
            _parameters.Add(new AnalyticsEventParameter(key, value));
            return this;
        }

        public AnalyticsEventBuilder Add(string key, float value)
        {
            return Add(key, (double)value);
        }

        public void Send()
        {
            AnalyticsService.SendEvent(_eventName, _parameters);
        }

        public void Dispose()
        {
            if (_pool != null)
            {
                _pool.Release(_parameters);
            }
        }

        private void ValidateDuplicateKey(string key)
        {
            for (int i = 0; i < _parameters.Count; i++)
            {
                if (_parameters[i].Key == key)
                {
                    throw new ArgumentException($"Analytics parameter '{key}' was already added.", nameof(key));
                }
            }
        }
    }
}
