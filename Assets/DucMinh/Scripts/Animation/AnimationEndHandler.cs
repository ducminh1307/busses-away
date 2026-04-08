using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DucMinh
{
    public class AnimationEndHandler : MonoBehaviour
    {
        private AnimatorAdapter adapter;
        private Animator animator;
        private Animation legacyAnimation;
        
        private readonly Dictionary<string, Action> callbacks = new();

        private void Awake()
        {
            animator = GetComponent<Animator>();
            legacyAnimation = GetComponent<Animation>();
        }

        private void PlayAnimation(string animationName, float speed, bool loop)
        {
            if (animator != null)
            {
                animator.speed = speed;
                animator.Play(animationName, 0, 0);
                if (loop)
                    Log.Warning("PlayAnimation(): Animator is not support set loop realtime.");
            }
            else if (legacyAnimation != null)
            {
                legacyAnimation.Play(animationName);
                var state = legacyAnimation[animationName];
                state.speed = speed;
                state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
            }
            else
            {
                Log.Warning($"PlayAnimation(): Not found animator on game object {gameObject.name}.");
            }
            
        }

        public void PlayAnimation(string animationName, Action onComplete, float speed, bool loop)
        {
            if (onComplete != null)
            {
                callbacks[animationName] = onComplete;
                AnimationClip clip = GetClip(animationName);
                if (clip != null)
                {
                    TryAddEvent(clip, animationName);
                }
            }
            PlayAnimation(animationName, speed, loop);
        }

        private AnimationClip GetClip(string animationName)
        {
            var clips = GetAllAnimationClips();
            return clips.FirstOrDefault(clip => clip.name == animationName);
        }

        private AnimationClip[] GetAllAnimationClips()
        {
            if (animator != null)
            {
                return animator.runtimeAnimatorController.animationClips;
            }

            if (legacyAnimation != null)
            {
                var clips = new AnimationClip[legacyAnimation.GetClipCount()];
                var index = 0;
                foreach (AnimationState animState in legacyAnimation)
                {
                    clips[index] = animState.clip;
                    index++;
                }
                return clips;
            }
            
            Log.Warning("GetAllAnimationClips(): animation clips not found.");
            return Array.Empty<AnimationClip>();
        }
        
        private void TryAddEvent(AnimationClip clip, string name)
        {
            int eventCount = clip.events.Length;
            if (clip.events.Length == 0)
            {
                AddEndEvent(clip, name);
            }
            else
            {
                var e = clip.events[eventCount - 1];

                if (Mathf.Approximately(e.time, clip.length))
                {
                    e.functionName = nameof(OnEndAnimation);
                    e.stringParameter = name;
                }
                else
                    AddEndEvent(clip, name);
            }
        }

        private void AddEndEvent(AnimationClip clip, string name)
        {
            AnimationEvent evt = new AnimationEvent
            {
                time = clip.length,
                functionName = nameof(OnEndAnimation),
                stringParameter = name
            };
            clip.AddEvent(evt);
        }

        private void OnEndAnimation(string name)
        {
            if (!callbacks.TryGetValue(name, out Action callback)) return;
            
            callback?.Invoke();
            // callbacks.Remove(name); // Xóa sau khi gọi
        }
    }
}