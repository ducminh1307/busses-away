using DucMinh;

namespace BussesAway
{
    public class CharacterStateRun: State<Character, CharacterStateID>
    {
        public CharacterStateRun(string animationName, Character entity) : base(animationName, entity)
        {
        }
    }
}