namespace DucMinh
{
    public interface IGameState
    {
        GameState GameState { get; }
        void OnPauseGame();
        void OnResumeGame();
        void OnLevelCompleted();
        void OnLevelLose();
        void OnRestartGame();
    }
}