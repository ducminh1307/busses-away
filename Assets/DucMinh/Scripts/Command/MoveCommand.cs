using System;
using UnityEngine;

namespace DucMinh.Command
{
    public class MoveCommand : IUndoableCommand
    {
        private readonly Transform _transform;
        private readonly Vector3 _previousPosition;
        private readonly Vector3 _targetPosition;
        private readonly Action<Transform, Vector3, Action> _moveAction;
        private readonly Action _onComplete;

        public MoveCommand(Transform transform, Vector3 targetPosition,
            Action<Transform, Vector3, Action> moveAction = null,
            Action onComplete = null)
        {
            _transform = transform;
            _previousPosition = transform.position;
            _targetPosition = targetPosition;
            _moveAction = moveAction;
            _onComplete = onComplete;
        }

        public void Execute()
        {
            if (_moveAction != null)
            {
                _moveAction(_transform, _targetPosition, _onComplete);
            }
            else
            {
                _transform.position = _targetPosition;
                _onComplete?.Invoke();
            }
        }

        public void Undo()
        {
            if (_moveAction != null)
            {
                _moveAction(_transform, _previousPosition, null);
            }
            else
            {
                _transform.position = _previousPosition;
            }
        }
    }
}