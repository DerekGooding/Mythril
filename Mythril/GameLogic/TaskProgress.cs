using Mythril.Data;

namespace Mythril.GameLogic;

public class TaskProgress(CardData cardData)
{
    public CardData CardData { get; } = cardData;
    public float ElapsedTime { get; private set; }
    public bool IsCompleted { get; private set; }

    public event Action<TaskProgress>? OnCompleted;

    public void Update(GameTime gameTime)
    {
        if (IsCompleted) return;

        ElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (ElapsedTime >= CardData.DurationSeconds)
        {
            IsCompleted = true;
            OnCompleted?.Invoke(this);
        }
    }
}
