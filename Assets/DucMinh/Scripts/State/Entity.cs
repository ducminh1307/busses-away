using System;
using UnityEngine;

namespace DucMinh
{
    public abstract class Entity<TStateEnum> : MonoBehaviour where TStateEnum : Enum
    {
        public AnimationAdapter AnimationAdapter { get; private set; }
        protected StateMachine StateMachine;
        protected StateFactory<TStateEnum> StateFactory;

        public TStateEnum CurrentStateID { get; protected set; }

        protected virtual void Awake()
        {
            StateMachine = new StateMachine();
            AnimationAdapter = AnimationAdapter.Create(gameObject);
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
            StateMachine?.CurrentState?.OnUpdate();
        }

        public virtual void ChangeState(TStateEnum stateID)
        {
            if (StateMachine.CurrentState != null && Equals(CurrentStateID, stateID)) return;

            var newState = StateFactory.GetState(stateID);
            CurrentStateID = stateID;

            if (StateMachine.CurrentState == null)
            {
                StateMachine.Initialize(newState);
            }
            else
            {
                StateMachine.ChangeState(newState);
            }
        }
    }
}