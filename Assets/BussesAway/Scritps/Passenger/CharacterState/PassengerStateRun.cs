using DucMinh;

namespace BussesAway
{
    public class PassengerStateRun: State<Passenger, PassengerStateID>
    {
        public PassengerStateRun(string animationName, Passenger entity) : base(animationName, entity)
        {
        }
    }
}