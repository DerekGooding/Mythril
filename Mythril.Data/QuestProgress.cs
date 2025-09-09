namespace Mythril.Data;

public class QuestProgress(string name, string description, int durationSeconds, IEnumerable<ItemQuantity> rewards, Character character)
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int DurationSeconds { get; set; } = durationSeconds;
    public Character Character { get; set; } = character;
    public DateTime StartTime { get; set; } = DateTime.Now;

    public IEnumerable<ItemQuantity> Rewards {  get; set; } = rewards;
    public double Progress { get; set; } = 0;
}
