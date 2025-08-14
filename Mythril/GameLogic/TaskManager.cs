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

        switch (task.CardData.Id)
        {
            case "card4": // Pray
                _resourceManager.AddFaith(task.CardData.RewardValue);
                Game1.Log($"Task '{task.CardData.Title}' completed. Gained {task.CardData.RewardValue} Faith.");
                break;
            case "card5": // Build Shrine
                _resourceManager.AddFaith(-10); // Consume 10 Faith
                _resourceManager.AddGold(-50); // Consume 50 Gold
                Game1.Log($"Task '{task.CardData.Title}' completed. Shrine built.");
                break;
            default:
                _resourceManager.AddGold(task.CardData.RewardValue); // Example reward
                Game1.Log($"Task '{task.CardData.Title}' completed. Gained {task.CardData.RewardValue} Gold.");
                break;
        }

        OnTaskCompleted?.Invoke(task);
    }

    public void Reset()
    {
        _activeTasks.Clear();
        _isPaused = false;
    }

    public IEnumerable<TaskProgress> GetActiveTasks() => _activeTasks;
}