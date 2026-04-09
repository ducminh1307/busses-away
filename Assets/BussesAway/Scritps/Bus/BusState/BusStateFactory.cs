using DucMinh;

namespace BussesAway
{
    public class BusStateFactory: StateFactory<BusStateID>
    {
        private Bus bus;
        public BusStateFactory(Bus bus)
        {
            this.bus = bus;
        }
        protected override IState CreateState(BusStateID id)
        {
            return id switch
            {
                BusStateID.Idle => new BusStateIdle(AnimationNames.Idle, bus),
                BusStateID.Run => new BusStateRun(AnimationNames.Run, bus),
                _ => null
            };
        }
    }
}