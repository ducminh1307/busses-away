using DucMinh;

namespace BussesAway
{
    public partial class GameController: IInputHandler<InputHit2D>
    {
        public bool OnTap(InputHit2D hit)
        {
            return true;
        }

        public bool OnDrag(InputHit2D hit)
        {
            return true;
        }

        public void OnRelease(InputHit2D hit)
        {
        }
    }
}