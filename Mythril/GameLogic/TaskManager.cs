using Mythril.Data;

namespace Mythril.GameLogic;

public class TaskManager(ResourceManager resourceManager)
{
    private readonly List<TaskProgress> _activeTasks = [];
    private readonly ResourceManager _resourceManager = resourceManager;
    private bool _isPaused = false;

    public event Action<TaskProgress>? OnTaskStarted;
    public event Action<TaskProgress>? OnTaskCompleted;

    public void SetPaused(bool paused) => _isPaused = paused;

    public void StartTask(TaskData taskData)
    {
        var task = new TaskProgress(taskData);
        task.OnCompleted += HandleTaskCompleted;
        _activeTasks.Add(task);
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

        switch (task.TaskData.Id)
        {
            case "task4": // Pray
                _resourceManager.AddFaith(task.TaskData.RewardValue);
                break;
            case "task5": // Build Shrine
                _resourceManager.AddFaith(-10); // Consume 10 Faith
                _resourceManager.AddGold(-50); // Consume 50 Gold
                break;
            default:
                _resourceManager.AddGold(task.TaskData.RewardValue); // Example reward
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