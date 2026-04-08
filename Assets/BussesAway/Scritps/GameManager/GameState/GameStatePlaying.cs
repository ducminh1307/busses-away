using DucMinh;
using PrimeTween;

namespace BussesAway
{
    public class GameStatePlaying: BaseGameState
    {
        private GameController _gameController;
        public GameStatePlaying(GameManager gameManager) : base(gameManager)
        {
        }

        public override void OnEnter()
        {
            Tween.Delay(0.1f, () =>
            {
                _gameController = "GameController".Create<GameController>();
                _gameController.LoadLevel(1);
            });
        }

        public override void OnExit()
        {
        }

        public override void OnUpdate()
        {
        }
    }
}