using System.Collections.Generic;

namespace DucMinh.Command
{
    public class CommandInvoker
    {
        private Stack<IUndoableCommand> _undoStack = new();
        private Stack<IUndoableCommand> _redoStack = new();

        public void ExecuteCommand(ICommand command)
        {
            if (command == null) return;

            command.Execute();
            if (command is IUndoableCommand undoableCommand)
            {
                _undoStack.Push(undoableCommand);
                // _redoStack.Clear();
            }
        }

        public void Undo()
        {
            if (_undoStack.Count <= 0) return;

            var activeCommand = _undoStack.Pop();
            activeCommand.Undo();

            _redoStack.Push(activeCommand);
        }

        public void Redo()
        {
            if (_redoStack.Count <= 0) return;
            var activeCommand = _redoStack.Pop();

            activeCommand.Execute();
            _undoStack.Push(activeCommand);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}