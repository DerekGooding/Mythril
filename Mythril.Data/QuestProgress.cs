namespace Mythril.Data;

public class QuestProgress(object item, string description, int durationSeconds, Character character)
{
    public object Item { get; set; } = item;

    public string Name { get; set; } =
        item is Quest quest ? quest.Name :
        item is CadenceUnlock unlock ? unlock.Ability.Name :
        string.Empty;

    public string Description { get; set; } = description;
    public int DurationSeconds { get; set; } = durationSeconds;
    public Character Character { get; set; } = character;
    public DateTime StartTime { get; set; } = DateTime.Now;

    public double Progress { get; set; } = 0;
}
