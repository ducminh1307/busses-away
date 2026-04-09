using DucMinh;

namespace BussesAway
{
    public class BusStateIdle: State<Bus, BusStateID>
    {
        public BusStateIdle(string animationName, Entity<BusStateID> entity) : base(animationName, entity)
        {
            
        }
    }
}