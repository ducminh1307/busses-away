using System;
using UnityEngine;

namespace DucMinh.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownDataAttribute : PropertyAttribute
    {
        public Type SourceType { get; private set; }
        public bool PreventDuplicates { get; private set; }

        public DropdownDataAttribute(Type sourceType, bool preventDuplicates = true)
        {
            SourceType = sourceType;
            PreventDuplicates = preventDuplicates;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DropdownRefAttribute : PropertyAttribute
    {
        public Type SourceType { get; private set; }
        public string ListName { get; private set; }
        public bool PreventDuplicates { get; private set; }

        public DropdownRefAttribute(Type sourceType, string listName, bool preventDuplicates = true)
        {
            SourceType = sourceType;
            ListName = listName;
            PreventDuplicates = preventDuplicates;
        }
    }
}