using System;

namespace DucMinh
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class InlineOrderAttribute : Attribute
    {
        public readonly string[] FieldNames;

        public InlineOrderAttribute(params string[] fieldNames)
        {
            FieldNames = fieldNames ?? Array.Empty<string>();
        }
    }
    
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class InlineLabelsAttribute : Attribute
    {
        public readonly string[] Labels;
        public InlineLabelsAttribute(params string[] labels)
        {
            Labels = labels ?? Array.Empty<string>();
        }
    }
    
    [Serializable]
    public abstract class TwoProperties { }
    
    [Serializable]
    public abstract class ThreeProperties { }
    
    [Serializable]
    public abstract class FourProperties { }
}