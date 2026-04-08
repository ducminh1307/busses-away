#define DEBUG_GAME_EVENT

using System;
using System.Collections.Generic;

namespace DucMinh
{
    public class GameEventSystem<E>  where E : Enum
    {
        private Delegate[] _eventTable = CreateEventTable();
#if DEBUG_GAME_EVENT
        private Dictionary<E, List<Delegate>> _debugEventTable = new();
#endif
        private static Delegate[] CreateEventTable()
        {
            var enumValues = Enum.GetValues(typeof(E));
            var enumLength = enumValues.Length;

#if UNITY_EDITOR
            var checks = new bool[enumLength];
            foreach (var enumValue in enumValues)
            {
                var index = Convert.ToInt32(enumValue);
                if (index < 0 || index >= enumLength)
                {
                    Log.Debug($"Enum value {enumValue} has an invalid index {index}.");
                }
                else
                {
                    if (checks[index])
                    {
                        Log.Debug("Duplicate enum index found: " + index);
                    }
                    else
                    {
                        checks[index] = true;
                    }
                }
            }

            for (int i = 0; i < enumLength; i++)
            {
                if (!checks[i]) Log.Debug("Missing enum index: " + i);
            }
#endif
            
            return new Delegate[enumLength];
        }

        #region Add

        public void AddEvent(E eventID, Action callback)
        {
            AddEventInternal(eventID, callback, dele => Delegate.Combine(dele, callback));
        }

        public void AddEvent<T>(E eventID, Action<T> callback)
        {
            AddEventInternal(eventID, callback, dele => Delegate.Combine(dele, callback));
        }
        
        public void AddEvent<T1, T2>(E eventID, Action<T1, T2> callback)
        {
            AddEventInternal(eventID, callback, dele => Delegate.Combine(dele, callback));
        }
        
        public void AddEvent<T1, T2, T3>(E eventID, Action<T1, T2, T3> callback)
        {
            AddEventInternal(eventID, callback, dele => Delegate.Combine(dele, callback));
        }
        
        private void AddEventInternal(E eventId, Delegate listener, Func<Delegate, Delegate> addHandler)
        {
#if DEBUG_GAME_EVENT
            if (!_debugEventTable.ContainsKey(eventId))
            {
                var eventList = new List<Delegate> { listener };
                _debugEventTable[eventId] = eventList;
                
            }
            else
            {
                var eventList = _debugEventTable[eventId];
                if (eventList.Contains(listener))
                {
                    Log.Debug($"Listener already added for event {eventId}");
                }
                else
                {
                    Log.Debug($"Add event {eventId}");
                    eventList.Add(listener);
                }
            }
#endif
            var index = Convert.ToInt32(eventId);
            var dele = _eventTable[index];
            if (dele == null)
            {
                _eventTable[index] = listener;
            }
            else
            {
                _eventTable[index] = addHandler(dele);
            }
        }

        #endregion

        #region Remove

        public void RemoveEvent(E eventID, Action callback)
        {
            RemoveEventInternal(eventID, callback, dele => Delegate.Remove(dele, callback));
        }
        
        public void RemoveEvent<T>(E eventID, Action<T> callback)
        {
            RemoveEventInternal(eventID, callback, dele => Delegate.Remove(dele, callback));
        }
        
        public void RemoveEvent<T1, T2>(E eventID, Action<T1, T2> callback)
        {
            RemoveEventInternal(eventID, callback, dele => Delegate.Remove(dele, callback));
        }
        
        public void RemoveEvent<T1, T2, T3>(E eventID, Action<T1, T2, T3> callback)
        {
            RemoveEventInternal(eventID, callback, dele => Delegate.Remove(dele, callback));
        }
        
        private void RemoveEventInternal(E eventID, Delegate listener, Func<Delegate, Delegate> removeHandler)
        {
#if DEBUG_GAME_EVENT
            if (_debugEventTable.TryGetValue(eventID, out var eventList))
            {
                if (eventList.Contains(listener))
                {
                    Log.Debug($"Remove event {eventID}");
                    eventList.Remove(listener);
                }
                else
                {
                    Log.Debug($"Listener not found for event {eventID}");
                }
            }
            else
            {
                Log.Debug($"No listeners found for event {eventID}");
            }
#endif
            var index = Convert.ToInt32(eventID);
            var dele = _eventTable[index];
            if (dele != null)
            {
                _eventTable[index] = removeHandler(dele);
            }
        }

        #endregion

        #region Broadcast

        public void Broadcast(E eventID)
        {
            BroadcastInternal(eventID, dele =>
            {
                if (dele is Action action)
                {
                    action.Invoke();
                }
#if DEBUG_GAME_EVENT
                else if (dele != null)
                {
                    Log.Debug($"Event {eventID} has a different signature: {dele.GetType()}");
                }
                
#endif
                else
                {
                    Log.Debug($"Event {eventID} has a different signature.");
                }
            });
        }
        
        public void Broadcast<T>(E eventID, T arg1)
        {
            BroadcastInternal(eventID, dele =>
            {
                if (dele is Action<T> action)
                {
                    action.Invoke(arg1);
                }
#if DEBUG_GAME_EVENT
                else if (dele != null)
                {
                    Log.Debug($"Event {eventID} has a different signature: {dele.GetType()}");
                }
                
#endif
                else
                {
                    Log.Debug($"Event {eventID} has a different signature.");
                }
            });
        }
        
        public void Broadcast<T1, T2>(E eventID, T1 arg1, T2 arg2)
        {
            BroadcastInternal(eventID, dele =>
            {
                if (dele is Action<T1, T2> action)
                {
                    action.Invoke(arg1, arg2);
                }
#if DEBUG_GAME_EVENT
                else if (dele != null)
                {
                    Log.Debug($"Event {eventID} has a different signature: {dele.GetType()}");
                }
                
#endif
                else
                {
                    Log.Debug($"Event {eventID} has a different signature.");
                }
            });
        }
        
        public void Broadcast<T1, T2, T3>(E eventID, T1 arg1, T2 arg2, T3 arg3)
        {
            BroadcastInternal(eventID, dele =>
            {
                if (dele is Action<T1, T2, T3> action)
                {
                    action.Invoke(arg1, arg2, arg3);
                }
#if DEBUG_GAME_EVENT
                else if (dele != null)
                {
                    Log.Debug($"Event {eventID} has a different signature: {dele.GetType()}");
                }
                
#endif
                else
                {
                    Log.Debug($"Event {eventID} has a different signature.");
                }
            });
        }

        private void BroadcastInternal(E eventID, Action<Delegate> broadcastAction)
        {
            var index = Convert.ToInt32(eventID);
            var dele = _eventTable[index];
            if (dele != null)
            {
                broadcastAction(dele);
            }
        }

        #endregion
    }
}