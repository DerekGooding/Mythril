namespace Mythril.Data;

public class QuestProgress(object item, string description, int durationSeconds, Character character, int slotIndex = 0)
{
    public object Item { get; set; } = item;
    public int SlotIndex { get; set; } = slotIndex;

    public string Name { get; set; } =
        item is QuestData quest ? quest.Name :
        item is CadenceUnlock unlock ? unlock.Ability.Name :
        item is RefinementData refinement ? refinement.Name :
        string.Empty;

    public string Description { get; set; } = description;
    public int DurationSeconds { get; set; } = durationSeconds;
    public Character Character { get; set; } = character;
    public DateTime StartTime { get; set; } = DateTime.Now;

    public double SecondsElapsed { get; set; } = 0;

    public double Progress => DurationSeconds > 0 ? Math.Clamp(SecondsElapsed / DurationSeconds, 0, 1) : 1;
    public bool IsCompleted => SecondsElapsed >= DurationSeconds;
}
