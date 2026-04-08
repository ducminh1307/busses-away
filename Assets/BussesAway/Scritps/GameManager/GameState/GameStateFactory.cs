using DucMinh;

namespace BussesAway
{
    public class GameStateFactory: StateFactory<GameStateID>
    {
        private readonly GameManager _gameManager;
        public GameStateFactory(GameManager gameManager)
        {
            _gameManager = gameManager;
        }
        
        protected override IState CreateState(GameStateID id)
        {
            return id switch
            {
                GameStateID.Playing => new GameStatePlaying(_gameManager),
                GameStateID.WinGame => new GameStateWin(_gameManager),
                GameStateID.LoseGame => new GameStateLose(_gameManager),
                _ => null
            };
        }
    }
}