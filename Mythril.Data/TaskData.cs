namespace Mythril.Data;

public class TaskData
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int DurationSeconds { get; set; }
    public int RewardValue { get; set; }

    public bool SingleUse { get; set; } = false;

    public Dictionary<string, int> Requirements { get; set; } = [];
    public Dictionary<string, int> Rewards { get; set; } = [];

    public List<string> Prerequisites { get; set; } = [];
}
