using UnityEngine;

namespace DucMinh.Attributes
{
    public class ElementNameAttribute : PropertyAttribute
    {
        public string Name { get; private set; }
        public System.Type EnumType { get; private set; }

        public ElementNameAttribute(string name)
        {
            Name = name;
        }

        public ElementNameAttribute(System.Type type)
        {
            EnumType = type;
        }
    }
}