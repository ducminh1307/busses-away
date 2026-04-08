using System;

namespace DucMinh
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public string Name { get; }

        public ButtonAttribute() { }

        public ButtonAttribute(string name)
        {
            Name = name;
        }
    }
}