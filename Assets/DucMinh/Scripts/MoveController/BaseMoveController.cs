using System;
using UnityEngine;

namespace DucMinh
{
    public class BaseMoveController: IMoveController
    {
        protected IMove _target;
        protected bool _isFinished = true;
        protected Vector3 _startPosition;
        protected Vector3 _endPosition;
        protected float _time = 0.5f;
        protected float _currentTime;
        protected Action _callback;
        
        public void SetTarget(IMove target) => _target = target;
        public bool IsFinished => _isFinished;

        public virtual void MoveTo(Vector3 position, float time = .5f, Action callback = null)
        {
            _startPosition = _target.GetPosition();
            _endPosition = position;
            _isFinished = false;
            _currentTime = 0;
            _time = time;
            _callback = callback;
        }

        public virtual void Update(float deltaTime)
        {
            if (!_isFinished && _target != null)
            {
                var newPosition = GetNewPosition(deltaTime);
                _target.SetPosition(newPosition);
            }
        }

        protected virtual Vector3 GetNewPosition(float deltaTime)
        {
            return _endPosition;
        }
    }
}