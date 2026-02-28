namespace Mythril.Data;

public class QuestData(Quest quest, QuestDetail detail)
{

    public Quest Quest { get; } = quest;

    public string Name => Quest.Name;
    public string Description => Quest.Description;

    public int DurationSeconds { get; init; } = detail.DurationSeconds;

    public ItemQuantity[] Requirements { get; init; } = detail.Requirements;
    public ItemQuantity[] Rewards { get; init; } = detail.Rewards;

    public QuestType Type { get; init; } = detail.Type;

}
