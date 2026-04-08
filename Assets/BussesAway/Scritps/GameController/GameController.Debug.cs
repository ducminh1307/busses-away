using System;
using DucMinh;

#if DEBUG_MODE
namespace BussesAway
{
    public partial class GameController
    {
        [Button]
        private void ForceWin()
        {
            gameManager?.ChangeState(GameStateID.WinGame);
        }
        
        [Button]
        private void ForceLose()
        {
            gameManager?.ChangeState(GameStateID.LoseGame);
        }
    }
}
#endif
