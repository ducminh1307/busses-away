using UnityEngine.UIElements;

namespace DucMinh
{
    public interface IDataController
    {
        string Key { get; }
        string Name { get; }
        void OnDebugGUI();
        void CreateDebugElement(VisualElement parent);
    }
    public interface IDataController<T> : IDataController
    {
        T Value { get; set; }
    }
}