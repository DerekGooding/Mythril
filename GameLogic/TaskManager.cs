namespace Mythril.GameLogic;

public class TaskManager(ResourceManager resourceManager)
{
    private readonly List<TaskProgress> _activeTasks = [];
    private readonly ResourceManager _resourceManager = resourceManager;
    private bool _isPaused = false;

    public event Action<TaskProgress>? OnTaskStarted;
    public event Action<TaskProgress>? OnTaskCompleted;

    public void SetPaused(bool paused) => _isPaused = paused;

    public void StartTask(CardData cardData)
    {
        var task = new TaskProgress(cardData);
        task.OnCompleted += HandleTaskCompleted;
        _activeTasks.Add(task);
        Game1.Log($"Task '{cardData.Title}' started.");
        OnTaskStarted?.Invoke(task);
    }

    public void Update(GameTime gameTime)
    {
        if (_isPaused)
        {
            return;
        }

        // Update active tasks
        foreach (var task in _activeTasks.ToList()) // ToList to avoid modification during iteration
        {
            task.Update(gameTime);
        }

        // Remove completed tasks (handled by HandleTaskCompleted)
    }

    private void HandleTaskCompleted(TaskProgress task)
    {
        _activeTasks.Remove(task);
        _resourceManager.AddGold(task.CardData.RewardValue); // Example reward
        Game1.Log($"Task '{task.CardData.Title}' completed. Gained {task.CardData.RewardValue} Gold.");
        OnTaskCompleted?.Invoke(task);
    }

    public void Reset()
    {
        _activeTasks.Clear();
        _isPaused = false;
    }
}