using DucMinh;

namespace BussesAway
{
    public class Character: Entity<CharacterStateID>
    {
        protected override void Awake()
        {
            StateFactory = new CharacterStateFactory(this);
            base.Awake();
        }
    }
}