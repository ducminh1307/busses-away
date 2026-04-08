using System;
using UnityEngine;

namespace DucMinh
{
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
    }

    public class State<T, TStateEnum> : IState
        where T : Entity<TStateEnum>
        where TStateEnum : Enum
    {
        protected float StateTimer;

        private readonly string animationName;
        protected bool TriggerCalled;

        protected readonly T Entity;

        protected State(string animationName, Entity<TStateEnum> entity)
        {
            this.animationName = animationName;
            Entity = entity as T;
        }

        public virtual void OnEnter()
        {
            TriggerCalled = false;
            Entity.AnimationAdapter.PlayAnimation(animationName);
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnUpdate()
        {
            StateTimer -= Time.deltaTime;
        }

        public virtual void AnimationTriggerCalled()
        {
            TriggerCalled = true;
        }
    }
}
