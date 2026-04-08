using DucMinh;
using PrimeTween;
using UnityEngine;

namespace BussesAway
{
    public partial class GameManager : SingletonBehavior<GameManager>
    {
        #region GameState

        GameStateFactory gameStateFactory;
        StateMachine stateMachine;
        public GameStateID CurrentState { get; private set; }

        #endregion
        
        protected override void Awake()
        {
            base.Awake();
            gameStateFactory = new GameStateFactory(this);
            stateMachine = new StateMachine();
            
            PrimeTweenConfig.warnZeroDuration = false;
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        }

        private void Start()
        {
            ChangeState(GameStateID.Playing);
        }

        private void Update()
        {
            stateMachine.CurrentState.OnUpdate();
        }

        public void ChangeState(GameStateID newStateID)
        {
            var newState = gameStateFactory.GetState(newStateID);
            if (newState == null) return;

            CurrentState = newStateID;
            if (stateMachine.CurrentState == null)
            {
                stateMachine.Initialize(newState);
                return;
            }

            if (stateMachine.CurrentState != newState)
            {
                stateMachine.ChangeState(newState);
            }
        }
    }
}
