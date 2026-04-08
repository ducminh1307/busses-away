using DucMinh;

namespace BussesAway
{
    public abstract class BaseGameState: IState
    {
        protected readonly GameManager GameManager;

        protected BaseGameState(GameManager gameManager)
        {
            GameManager = gameManager;
        }

        public abstract void OnEnter();

        public abstract void OnExit();

        public abstract void OnUpdate();
    }
}