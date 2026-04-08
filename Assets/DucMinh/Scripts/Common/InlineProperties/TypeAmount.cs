using UnityEngine;

namespace DucMinh
{
    [System.Serializable]
    [InlineLabels("Type", "Amount")]
    public class TypeAmount<T> : TwoProperties where T : System.Enum
    {
        [SerializeField] private T _type;
        [SerializeField] private int _amount;
        
        public T Type => _type;
        public int Amount => _amount;
    }
}