#if DEBUG_MODE && UNITY_EDITOR
using DucMinh;

namespace BussesAway
{
    public partial class Bus
    {
        [Button]
        private void TestAnimationDoor(bool open)
        {
            ChangeDoor(null, open);
        }
        
        [Button]
        private void TestBusFull(bool full)
        {
            SetFull(full);
        }
    }
}
#endif
