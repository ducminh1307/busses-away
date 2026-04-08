using System;
using UnityEngine;

namespace DucMinh
{
    public class LinearMoveController : BaseMoveController
    {
        public override void MoveTo(Vector3 position, float time = 0.5f, Action callback = null)
        {
            base.MoveTo(position, time, callback);
            _currentTime = 0;
        }

        protected override Vector3 GetNewPosition(float deltaTime)
        {
            _currentTime += deltaTime;
            var t = Mathf.Clamp01(_currentTime / _time);
            if (Mathf.Approximately(t, 1))
            {
                _callback?.Invoke();
                _isFinished = true;
            }
            return Vector3.Lerp(_startPosition, _endPosition, t);
        }
    }
}