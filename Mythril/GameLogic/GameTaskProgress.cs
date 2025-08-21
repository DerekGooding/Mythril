using Mythril.Data;

namespace Mythril.GameLogic;

public class GameTaskProgress(TaskData taskData)
{
    public TaskData TaskData { get; } = taskData;
    public float ElapsedTime { get; private set; }
    public bool IsCompleted { get; private set; }

    public event Action<GameTaskProgress>? OnCompleted;

    public void Update(GameTime gameTime)
    {
        if (IsCompleted) return;

        ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ElapsedTime >= TaskData.DurationSeconds)
        {
            IsCompleted = true;
            OnCompleted?.Invoke(this);
        }
    }
}
