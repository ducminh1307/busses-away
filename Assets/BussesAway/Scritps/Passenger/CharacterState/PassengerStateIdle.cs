using DucMinh;

namespace BussesAway
{
    public class PassengerStateIdle: State<Passenger, PassengerStateID>
    {
        public PassengerStateIdle(string animationName, Passenger entity) : base(animationName, entity)
        {
        }
    }
}