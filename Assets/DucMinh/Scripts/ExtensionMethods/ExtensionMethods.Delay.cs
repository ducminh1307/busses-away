using System;
using System.Collections;
using UnityEngine;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void ExecuteNextFrame(this MonoBehaviour mono, Action action)
        {
            if (mono == null)
            {
                action?.Invoke();
                return;
            }
            
            mono.StartCoroutine(OnNextFrame());
            return;
            
            IEnumerator OnNextFrame()
            {
                yield return null;
                action?.Invoke();
            }
        }

        public static void ExecuteNextFrames(this MonoBehaviour mono, Action action, int frames)
        {
            if (mono == null)
            {
                action?.Invoke();
                return;
            }
            
            mono.StartCoroutine(OnNextFrame());
            return;
            
            IEnumerator OnNextFrame()
            {
                for (int i = 0; i < frames; i++)
                    yield return null;
                action?.Invoke();
            }
        }
        
        public static void ExecuteNextSeconds(this MonoBehaviour mono, Action action, float seconds)
        {
            if (mono == null)
            {
                action?.Invoke();
                return;
            }
            
            mono.StartCoroutine(OnNextFrame());
            return;
            
            IEnumerator OnNextFrame()
            {
                yield return new WaitForSeconds(seconds);
                action?.Invoke();
            }
        }
    }
}