namespace DucMinh.Command
{
    public interface ICommand
    {
        void Execute();
    }
    
    public interface IUndoableCommand: ICommand
    {
        void Undo();
    }
}