using System;
using System.Collections.Generic;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void Loop(this int count, Action<int> callback)
        {
            for (var i = 0; i < count; i++)
            {
                callback?.Invoke(i);
            }
        }

        public static void Loop<T>(this IList<T> list, Action<T> callback)
        {
            foreach (var t in list)
            {
                callback?.Invoke(t);
            }
        }
        
        public static void Loop<T>(this IList<T> list, Action<T, int> callback)
        {
            for (var i = 0; i < list.Count; i++)
            {
                callback?.Invoke(list[i], i);
            }
        }
        
        public static void LoopReverse<T>(this IList<T> list, Action<T> callback)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                callback?.Invoke(list[i]);
            }
        }
        
        public static void LoopReverse<T>(this IList<T> list, Action<T, int> callback)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                callback?.Invoke(list[i], i);
            }
        }
        
        public static void LoopBreakable(this int count, Func<int, bool> condition)
        {
            for (var i = 0; i < count; i++)
            {
                if (condition?.Invoke(i) == true)
                {
                    break;
                }
            }
        }

        public static void LoopBreakable<T>(this IList<T> list, Func<T, int, bool> condition)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (condition?.Invoke(list[i], i) == true)
                {
                    break;
                }
            }
        }
    }
}