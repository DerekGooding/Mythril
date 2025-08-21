namespace Mythril.Data;

public class TaskProgress(TaskData task, Character character)
{
    public TaskData Task { get; set; } = task;
    public Character Character { get; set; } = character;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public double Progress { get; set; } = 0;
}
