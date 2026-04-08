namespace DucMinh
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void Initialize(IState initialState)
        {
            CurrentState = initialState;
            CurrentState.OnEnter();
        }
        
        public void ChangeState(IState newState)
        {
            CurrentState.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }
    }
}