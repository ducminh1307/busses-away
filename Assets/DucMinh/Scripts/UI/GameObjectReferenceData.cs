using System;
using System.Collections.Generic;
using UnityEngine;

namespace DucMinh
{
    [Serializable]
    public class GameObjectReferenceData
    {
        public string name;
        public GameObject gameObject;

        public static readonly List<string> NameObjectReferences = new()
        {
            "Root",
            "Top",
            "Bottom",
            "Left",
            "Right",
            "Button",
            "Text",
            "Image",
            "Slider",
            "Icon",
            "Toggle",
            "Tab",
            "TabButton",
            "TabContent",
            "Close Button",
            "Content",
            "Title",
            "Sub Text",
            "Background",
        };
    }

    public static class GameObjectReferenceID
    {
        public const string Root = "Root";
        public const string Top = "Top";
        public const string Bottom = "Bottom";
        public const string Left = "Left";
        public const string Right = "Right";
        public const string Button = "Button";
        public const string Text = "Text";
        public const string Image = "Image";
        public const string Slider = "Slider";
        public const string Icon = "Icon";
        public const string Toggle = "Toggle";
        public const string Tab = "Tab";
        public const string TabButton = "TabButton";
        public const string TabContent = "TabContent";
        public const string CloseButton = "Close Button";
        public const string Content = "Content";
        public const string Title = "Title";
        public const string SubText = "Sub Text";
        public const string Background = "Background";
    }
}