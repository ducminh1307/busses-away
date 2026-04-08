using DucMinh;

namespace BussesAway
{
    public class CharacterStateIdle: State<Character, CharacterStateID>
    {
        public CharacterStateIdle(string animationName, Character entity) : base(animationName, entity)
        {
        }
    }
}