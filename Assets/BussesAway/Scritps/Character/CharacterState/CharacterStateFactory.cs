using DucMinh;

namespace BussesAway
{
    public class CharacterStateFactory: StateFactory<CharacterStateID>   
    {
        private Character _character;
        public CharacterStateFactory(Character character)
        {
            _character = character;
        }
        protected override IState CreateState(CharacterStateID id)
        {
            return id switch
            {
                CharacterStateID.Idle => new CharacterStateIdle(AnimationNames.Idle, _character),
                CharacterStateID.Running => new CharacterStateRun(AnimationNames.Run, _character),
                _ => null
            };
        }
    }
}