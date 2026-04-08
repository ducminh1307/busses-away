using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DucMinh
{
    public static partial class AnalyticsService
    {
        private static readonly List<IAnalyticsProvider> _providers = new();
        public static bool Initialized { get; private set; } = false;

        private static string _pendingUserId = null;
        private static readonly ConcurrentQueue<PendingEvent> _pendingEvents = new();
        private static readonly ConcurrentQueue<PendingProperty> _pendingProperties = new();

        private static readonly DelegatedPool<List<AnalyticsEventParameter>> _parameterPool = new(
            create: () => new List<AnalyticsEventParameter>(8),
            onGet: list => list.Clear(),
            onRelease: list => list.Clear(),
            onDestroy: list => list.Clear()
        );

        public static void Initialize(IEnumerable<IAnalyticsProvider> providers)
        {
            if (Initialized) return;

            if (providers.IsNullOrEmpty())
            {
                Log.Warning($"[Analytics] No analytics providers were added during initialization.");
                Initialized = true;
                return;
            }

            _parameterPool.AutoExpand = true;
            _parameterPool.MaxSize = 50;

            foreach (var provider in providers)
            {
                provider.Initialize();
                _providers.Add(provider);
            }

            Initialized = true;
            ProcessingData();
        }

        private static void ProcessingData()
        {
            if (!string.IsNullOrEmpty(_pendingUserId))
            {
                foreach (var provider in _providers)
                {
                    if (provider.Initialized) provider.SetUserId(_pendingUserId);
                }
            }

            while (_pendingEvents.TryDequeue(out var pendingEvent))
            {
                SendEvent(pendingEvent.EventName, pendingEvent.Parameters);
            }

            while (_pendingProperties.TryDequeue(out var pendingProp))
            {
                SetUserProperty(pendingProp.PropertyName, pendingProp.Parameter);
            }
        }

        public static void SetUserId(string userId)
        {
            _pendingUserId = userId;

            if (!Initialized) return;

            foreach (var provider in _providers)
            {
                try
                {
                    if (provider.Initialized)
                        provider.SetUserId(userId);
                }
                catch (Exception e)
                {
                    Log.Error($"[Analytics] Failed to set UserId {userId}: {e.Message}");
                }
            }
        }

        public static AnalyticsEventBuilder BuildEvent(string eventName)
        {
            return new AnalyticsEventBuilder(eventName, _parameterPool.Get(), _parameterPool);
        }

        public static void SendEvent(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters = null)
        {
            if (!Initialized)
            {
                _pendingEvents.Enqueue(new PendingEvent(eventName, CloneParameters(parameters)));
                return;
            }

            foreach (var provider in _providers)
            {
                try
                {
                    if (provider.Initialized)
                    {
                        provider.LogEvent(eventName, parameters);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[Analytics] Failed to log {eventName}: {e.Message}");
                }
            }
        }

        public static void SendEvent(string eventName, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                SendEvent(eventName, (IReadOnlyList<AnalyticsEventParameter>)null);
                return;
            }

            var convertedParameters = new List<AnalyticsEventParameter>(parameters.Count);
            foreach (var parameter in parameters)
            {
                convertedParameters.Add(AnalyticsEventParameter.FromObject(parameter.Key, parameter.Value));
            }

            SendEvent(eventName, convertedParameters);
        }

        public static void SetUserProperty(string propertyName, object propertyValue)
        {
            if (!Initialized)
            {
                _pendingProperties.Enqueue(new PendingProperty(propertyName, propertyValue));
                return;
            }

            foreach (var provider in _providers)
            {
                try
                {
                    if (provider.Initialized)
                        provider.SetUserProperty(propertyName, propertyValue);
                }
                catch (Exception e)
                {
                    Log.Error($"[Analytics] Failed to set property {propertyName}: {e.Message}");
                }
            }
        }

        private static IReadOnlyList<AnalyticsEventParameter> CloneParameters(IReadOnlyList<AnalyticsEventParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0) return null;

            var clone = new AnalyticsEventParameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                clone[i] = parameters[i];
            }

            return clone;
        }

        private readonly struct PendingEvent
        {
            public readonly string EventName;
            public readonly IReadOnlyList<AnalyticsEventParameter> Parameters;

            public PendingEvent(string eventName, IReadOnlyList<AnalyticsEventParameter> parameters)
            {
                EventName = eventName;
                Parameters = parameters;
            }
        }

        private readonly struct PendingProperty
        {
            public readonly string PropertyName;
            public readonly object Parameter;

            public PendingProperty(string propertyName, object parameter)
            {
                PropertyName = propertyName;
                Parameter = parameter;
            }
        }
    }
}
