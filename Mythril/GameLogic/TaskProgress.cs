using Microsoft.Xna.Framework;
using Mythril.Data;
using System;

namespace Mythril.GameLogic;

public class TaskProgress
{
    public CardData CardData { get; }
    public float ElapsedTime { get; private set; }
    public bool IsCompleted { get; private set; }

    public event Action<TaskProgress>? OnCompleted;

    public TaskProgress(CardData cardData)
    {
        CardData = cardData;
    }

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
