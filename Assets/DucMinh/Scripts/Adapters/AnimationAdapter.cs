using System;
using System.Collections.Generic;
using UnityEngine;
#if SPINE_ANIMATION
using Spine.Unity;
#endif

namespace DucMinh
{
    public abstract class AnimationAdapter
    {
        public abstract GameObject GameObject { get; }
        public abstract void PlayAnimation(string animationName, float speed = 1f, bool loop = false);
        public abstract void Stop();
        public abstract bool IsPlaying(string animationName);
        public abstract string CurrentAnimation { get; }
        public abstract float Speed { get; set; }
        public abstract float AnimationLength(string animationName);

        public abstract void PlayWithCallback(string animationName, Action onComplete = null, float speed = 1f,
            bool loop = false);

        #region Static Method

        public static void PlayAnimation(GameObject gObj, string animationName, float speed = 1f, bool loop = false)
        {
            PlayAnimationWithCallback(gObj, animationName, null, speed, loop);
        }

        public static void PlayAnimationWithCallback(GameObject gObj, string animationName, Action onComplete = null,
            float speed = 1f, bool loop = false)
        {
            if (gObj == null)
            {
                Log.Error($"PlayAnimation({animationName}): GameObject is null");
                return;
            }

            var adapter = Create(gObj);
            if (adapter != null)
                adapter.PlayWithCallback(animationName, onComplete, speed, loop);
            else
                Log.Error($"No supported animation component found on {gObj.name}");
        }

        public static AnimationAdapter Create(GameObject gObj)
        {
            if (gObj == null)
            {
                Log.Error("AnimationAdapter:Create(): GameObject is null");
                return null;
            }

            var animator = gObj.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                return new AnimatorAdapter(animator);
            }

            var animation = gObj.GetComponentInChildren<Animation>();
            if (animation != null)
            {
                return new LegacyAnimationAdapter(animation);
            }

#if SPINE_ANIMATION
            var spineSkeleton = gObj.GetComponentInChildren<SkeletonAnimation>();
            if (spineSkeleton != null)
            {
                return new SpineAdapter(spineSkeleton);
            }
#endif
            Log.Warning($"No supported animation component found on {gObj.name}");

            return null;
        }

        public static float AnimationLength(GameObject gObj, string animationName)
        {
            var adapter = Create(gObj);
            return adapter?.AnimationLength(animationName) ?? 0f;
        }

        public static AnimationAdapter Create(Animator animator)
        {
            return new AnimatorAdapter(animator);
        }

        public static AnimationAdapter Create(Animation animation)
        {
            return new LegacyAnimationAdapter(animation);
        }

#if SPINE_ANIMATION
        public static AnimationAdapter Create(SkeletonAnimation skeletonAnimation)
        {
            return new SpineAdapter(skeletonAnimation); 
        }
#endif

        #endregion
    }
    
    public class AnimatorAdapter : AnimationAdapter
    {
        private readonly Animator animator;
        private readonly AnimationEndHandler endHandler;

        public AnimatorAdapter(Animator animator)
        {
            this.animator = animator;
            endHandler = animator.GetOrAddComponent<AnimationEndHandler>();
        }

        public override GameObject GameObject => animator.gameObject;

        public override void PlayAnimation(string animationName, float speed = 1, bool loop = false)
        {
            if (loop)
                Log.Warning("Animator is not support set loop realtime.");
            animator.speed = speed;
            animator.Play(animationName, 0 , 0);
        }

        public override void Stop()
        {
            if (!animator.isActiveAndEnabled) return;
            animator.speed = 0f;
        }

        public override bool IsPlaying(string animationName)
        {
            if (!animator.isActiveAndEnabled) return false;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsName(animationName) && info.normalizedTime < 1f;
        }

        public override string CurrentAnimation
        {
            get
            {
                if (!animator.isActiveAndEnabled) return string.Empty;
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                return clipInfo.Length > 0 ? clipInfo[0].clip.name : string.Empty;
            }
        }

        public override float Speed
        {
            get => animator ? animator.speed : 0f;
            set => animator.speed = value > 0f ? value : 0f;
        }

        public override float AnimationLength(string animationName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName) return clip.length;
            }

            Log.Warning($"Animation clip '{animationName}' not found.");
            return 0f;
        }

        public override void PlayWithCallback(string animationName, Action onComplete = null, float speed = 1,
            bool loop = false)
        {
            if (animator == null || !animator.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return;
            }

            endHandler.PlayAnimation(animationName, onComplete, speed, loop);
        }

        #region Parameter
        
        public void SetParameter(string parameterName, bool value)
        {
            if (animator.isActiveAndEnabled)
                animator.SetBool(parameterName, value);
        }

        public void SetParameter(string parameterName, float value)
        {
            if (animator.isActiveAndEnabled)
                animator.SetFloat(parameterName, value);
        }

        public void SetParameter(string parameterName, int value)
        {
            if (animator.isActiveAndEnabled)
                animator.SetInteger(parameterName, value);
        }

        public void PlayWithParameters(string animationName,
            (string name, bool value)[] boolParams = null, (string name, int value)[] intParams = null,
            (string name, float value)[] floatParams = null,
            Action onComplete = null, float speed = 1f, bool loop = false)
        {
            if (!animator.isActiveAndEnabled)
            {
                Log.Error("Animator is null or disabled");
                onComplete?.Invoke();
                return;
            }

            if (boolParams != null)
                foreach (var param in boolParams)
                    animator.SetBool(param.name, param.value);

            if (intParams != null)
                foreach (var param in intParams)
                    animator.SetInteger(param.name, param.value);

            if (floatParams != null)
                foreach (var param in floatParams)
                    animator.SetFloat(param.name, param.value);

            endHandler.PlayAnimation(animationName, onComplete, speed, loop);
        }

        #endregion
    }

    public class LegacyAnimationAdapter : AnimationAdapter
    {
        private readonly Animation animation;
        private readonly AnimationEndHandler endHandler;

        public LegacyAnimationAdapter(Animation animation)
        {
            this.animation = animation;
            endHandler = animation.GetOrAddComponent<AnimationEndHandler>();
        }

        public override GameObject GameObject => animation.gameObject;

        public override void PlayAnimation(string animationName, float speed = 1, bool loop = false)
        {
            var clip = animation.GetClip(animationName);
            if (clip != null)
            {
                AnimationState state = animation[animationName];
                state.speed = speed;
                state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                animation.Play(animationName);
            }
            else
                Log.Error($"Animation clip named \"{animationName}\" not found.");
        }

        public override void Stop()
        {
            animation.Stop();
        }

        public override bool IsPlaying(string animationName)
        {
            return animation.IsPlaying(animationName);
        }

        public override string CurrentAnimation => animation[animation.clip.name].name;

        public override float Speed
        {
            get => animation[animation.clip.name].speed;
            set => animation[animation.clip.name].speed = value;
        }

        public override float AnimationLength(string animationName)
        {
            if (animation.GetClip(animationName) != null)
            {
                return animation[animationName].length;
            }

            Log.Error($"Animation clip named \"{animationName}\" not found.");
            return 0f;
        }

        public override void PlayWithCallback(string animationName, Action onComplete = null, float speed = 1, bool loop = false)
        {
            if (animation == null || !animation.isActiveAndEnabled)
            {
                onComplete?.Invoke();
                return;
            }

            endHandler.PlayAnimation(animationName, onComplete, speed, loop);
        }
    }

#if SPINE_ANIMATION
    public class SpineAdapter : AnimationAdapter
    {
        private readonly SkeletonAnimation skeletonAnimation;

        public SpineAdapter(SkeletonAnimation skeletonAnimation)
        {
            this.skeletonAnimation = skeletonAnimation;
        }

        public override GameObject GameObject => skeletonAnimation.gameObject;

        public override void Play(string animationName, float speed = 1f, bool loop = false)
        {
            if (!skeletonAnimation.isActiveAndEnabled) return;
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            skeletonAnimation.AnimationState.TimeScale = speed;
        }

        public override void Stop()
        {
            if (skeletonAnimation != null && skeletonAnimation.isActiveAndEnabled)
                skeletonAnimation.AnimationState.ClearTracks();
        }

        public override bool IsPlaying(string animationName)
        {
            if (skeletonAnimation == null || !skeletonAnimation.isActiveAndEnabled) return false;
            var track = skeletonAnimation.AnimationState.GetCurrent(0);
            return track != null && track.Animation.Name == animationName && track.TrackTime < track.AnimationEnd;
        }

        public override string CurrentAnimation
        {
            get
            {
                if (skeletonAnimation == null || !skeletonAnimation.isActiveAndEnabled) return string.Empty;
                var track = skeletonAnimation.AnimationState.GetCurrent(0);
                return track != null ? track.Animation.Name : string.Empty;
            }
        }

        public override float Speed
        {
            get => skeletonAnimation != null ? skeletonAnimation.AnimationState.TimeScale : 1f;
            set { if (skeletonAnimation != null) skeletonAnimation.AnimationState.TimeScale = value; }
        }

        public override void PlayWithEndCallback(string animationName, Action onComplete = null, float speed =
 1f, bool loop = false)
        {
            if (skeletonAnimation == null || !skeletonAnimation.isActiveAndEnabled)
            {
                Log.Error("SkeletonAnimation is null or disabled.");
                onComplete?.Invoke();
                return;
            }

            skeletonAnimation.AnimationState.Complete -= HandleSpineComplete;
            if (onComplete != null && !loop)
            {
                skeletonAnimation.AnimationState.Complete += HandleSpineComplete;
            }

            void HandleSpineComplete(Spine.TrackEntry trackEntry)
            {
                if (trackEntry.Animation.Name == animationName)
                {
                    onComplete?.Invoke();
                    skeletonAnimation.AnimationState.Complete -= HandleSpineComplete;
                }
            }

            skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
            skeletonAnimation.AnimationState.TimeScale = speed;
        }

        public override float GetAnimationLength(string animationName)
        {
            if (skeletonAnimation == null || skeletonAnimation.SkeletonDataAsset == null) return 0f;
            var skeletonData = skeletonAnimation.SkeletonDataAsset.GetSkeletonData(true);
            var animation = skeletonData.FindAnimation(animationName);
            if (animation != null) return animation.Duration;
            Log.Warning($"Spine animation '{animationName}' not found.");
            return 0f;
        }
    }
#endif
}

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static void PlayAnimation(this GameObject gObj, string animationName, float speed = 1f, bool loop = false)
        {
            AnimatorAdapter.PlayAnimation(gObj, animationName, speed, loop);
        }

        public static void PlayAnimation(this GameObject gObj, string animationName, Action onComplete = null, float speed = 1f, bool loop = false)
        {
            AnimatorAdapter.PlayAnimationWithCallback(gObj, animationName, onComplete, speed, loop);
        }
    }
}