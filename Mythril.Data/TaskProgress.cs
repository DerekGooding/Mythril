namespace Mythril.Data;

public class TaskProgress
{
    public TaskData Task { get; set; }
    public Character Character { get; set; }
    public DateTime StartTime { get; set; }
    public double Progress { get; set; }

    public TaskProgress(TaskData task, Character character)
    {
        Task = task;
        Character = character;
        StartTime = DateTime.Now;
        Progress = 0;
    }
}
