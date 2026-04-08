using UnityEngine;

namespace DucMinh
{
    public interface IMove
    {
        void SetPosition(Vector3 position);
        void SetAngle(float angle);
        void SetScale(Vector3 scale);
        Vector3 GetPosition();
    }
}