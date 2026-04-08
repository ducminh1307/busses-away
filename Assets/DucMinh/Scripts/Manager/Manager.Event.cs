using System;

namespace DucMinh
{
    public partial class Manager
    {
        private static GameEventSystem<GameEventID> eventSystem = new();

        #region Add

        public static void AddEvent(GameEventID eventID, Action callback)
        {
            eventSystem.AddEvent(eventID, callback);
        }
        
        public static void AddEvent<T>(GameEventID eventID, Action<T> callback)
        {
            eventSystem.AddEvent(eventID, callback);
        }
        
        public static void AddEvent<T1, T2>(GameEventID eventID, Action<T1, T2> callback)
        {
            eventSystem.AddEvent(eventID, callback);
        }
        
        public static void AddEvent<T1, T2, T3>(GameEventID eventID, Action<T1, T2, T3> callback)
        {
            eventSystem.AddEvent(eventID, callback);
        }

        #endregion

        #region Broadcast

        public static void BroadcastEvent(GameEventID eventID)
        {
            eventSystem.Broadcast(eventID);
        }
        
        public static void BroadcastEvent<T>(GameEventID eventID, T param)
        {
            eventSystem.Broadcast(eventID, param);
        }
        
        public static void BroadcastEvent<T1, T2>(GameEventID eventID, T1 param1, T2 param2)
        {
            eventSystem.Broadcast(eventID, param1, param2);
        }
        
        public static void BroadcastEvent<T1, T2, T3>(GameEventID eventID, T1 param1, T2 param2, T3 param3)
        {
            eventSystem.Broadcast(eventID, param1, param2, param3);
        }
        
        #endregion

        #region Remove

        public static void RemoveEvent(GameEventID eventID, Action callback)
        {
            eventSystem.RemoveEvent(eventID, callback);
        }
        
        public static void RemoveEvent<T>(GameEventID eventID, Action<T> callback)
        {
            eventSystem.RemoveEvent(eventID, callback);
        }
        
        public static void RemoveEvent<T1, T2>(GameEventID eventID, Action<T1, T2> callback)
        {
            eventSystem.RemoveEvent(eventID, callback);
        }
        
        public static void RemoveEvent<T1, T2, T3>(GameEventID eventID, Action<T1, T2, T3> callback)
        {
            eventSystem.RemoveEvent(eventID, callback);
        }

        #endregion
    }

    public static partial class ExtensionMethods
    {
        public static void AddEvent(this GameEventID eventID, Action callback) => Manager.AddEvent(eventID, callback);
        public static void AddEvent<T>(this GameEventID eventID, Action<T> callback) => Manager.AddEvent(eventID, callback);
        public static void AddEvent<T1, T2>(this GameEventID eventID, Action<T1, T2> callback) => Manager.AddEvent(eventID, callback);
        public static void AddEvent<T1, T2, T3>(this GameEventID eventID, Action<T1, T2, T3> callback) => Manager.AddEvent(eventID, callback);

        public static void BroadCast(this GameEventID eventID) => Manager.BroadcastEvent(eventID);
        public static void BroadCast<T>(this GameEventID eventID, T param) => Manager.BroadcastEvent(eventID, param);
        public static void BroadCast<T1, T2>(this GameEventID eventID, T1 param1, T2 param2) => Manager.BroadcastEvent(eventID, param1, param2);
        public static void BroadCast<T1, T2, T3>(this GameEventID eventID, T1 param1, T2 param2, T3 param3) => Manager.BroadcastEvent(eventID, param1, param2, param3);

        public static void RemoveEvent(this GameEventID eventID, Action callback) => Manager.RemoveEvent(eventID, callback);
        public static void RemoveEvent<T>(this GameEventID eventID, Action<T> callback) => Manager.RemoveEvent(eventID, callback);
        public static void RemoveEvent<T1, T2>(this GameEventID eventID, Action<T1, T2> callback) => Manager.RemoveEvent(eventID, callback);
        public static void RemoveEvent<T1, T2, T3>(this GameEventID eventID, Action<T1, T2, T3> callback) => Manager.RemoveEvent(eventID, callback);
    }
}