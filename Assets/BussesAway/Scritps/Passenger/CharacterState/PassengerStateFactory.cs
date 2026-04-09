using DucMinh;

namespace BussesAway
{
    public class PassengerStateFactory: StateFactory<PassengerStateID>   
    {
        private Passenger _passenger;
        public PassengerStateFactory(Passenger passenger)
        {
            _passenger = passenger;
        }
        protected override IState CreateState(PassengerStateID id)
        {
            return id switch
            {
                PassengerStateID.Idle => new PassengerStateIdle(AnimationNames.Idle, _passenger),
                PassengerStateID.Running => new PassengerStateRun(AnimationNames.Run, _passenger),
                _ => null
            };
        }
    }
}