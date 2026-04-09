using DucMinh;
using UnityEngine;

namespace BussesAway
{
    public partial class Passenger: Entity<PassengerStateID>
    {
        [SerializeField] private GameObject renderObj;
        
        public ColorType CurrentColor { get; private set; }
        
        protected override void Awake()
        {
            StateFactory = new PassengerStateFactory(this);
            AnimationAdapter = AnimationAdapter.Create(renderObj);
            base.Awake();
        }

        public void Construct(ColorType colorType)
        {
            CurrentColor = colorType;
        }
    }
}
