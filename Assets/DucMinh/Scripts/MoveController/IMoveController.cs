using System;
using UnityEngine;

namespace DucMinh
{
    public interface IMoveController
    {
        void SetTarget(IMove target);
        void MoveTo(Vector3 position, float time, Action callback = null);
        void Update(float deltaTime);
    }
}
