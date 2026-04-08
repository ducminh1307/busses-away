using UnityEngine;

namespace BussesAway
{
    public partial class GameController
    {
        public Bounds GetBoardBounds()
        {
            return new Bounds(Vector2.zero, Vector2.one);
        }
    }
}