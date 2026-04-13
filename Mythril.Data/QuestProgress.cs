namespace Mythril.Data;

public record QuestProgress(object Item, string Description, int DurationSeconds, Character Character, int SlotIndex = 0)
{
    public string Name =>
        Item is QuestData quest ? quest.Name :
        Item is CadenceUnlock unlock ? unlock.Ability.Name :
        Item is RefinementData refinement ? refinement.Name :
        string.Empty;

    public string PrimaryStat =>
        Item is QuestData q ? q.PrimaryStat :
        Item is CadenceUnlock u ? u.PrimaryStat :
        Item is RefinementData r ? r.PrimaryStat :
        "Vitality"; // Default

    public DateTime StartTime { get; set; } = DateTime.Now;

    public double SecondsElapsed { get; set; } = 0;

    public double Progress => DurationSeconds > 0 ? Math.Clamp(SecondsElapsed / DurationSeconds, 0, 1) : 1;
    public bool IsCompleted => SecondsElapsed >= DurationSeconds;
}
