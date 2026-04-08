using UnityEngine;

namespace DucMinh
{
    public interface IComponent
    {
        GameObject gameObject { get; }
        Transform transform { get; }
    }
}