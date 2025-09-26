using static Mythril.Data.QuestDetailsBuilder;

namespace Mythril.Data;

public readonly record struct QuestDetail(int DurationSeconds, ItemQuantity[] Requirements, ItemQuantity[] Rewards, QuestType Type);

[Singleton]
public class QuestDetails(Quests quests, Items items) : ISubContent<Quest, QuestDetail>
{
    public QuestDetail this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : new(3, [], [], QuestType.Single);

    public Dictionary<Quest, QuestDetail> ByKey { get; } = new()
    {
        { quests.TutorialSection, new(3, [], [new(items.BasicGem)], QuestType.Recurring)  },
        { quests.BuyPotion, new(3, [new(items.Gold, 250)], [new(items.Potion)], QuestType.Recurring) },
        { quests.FarmGoblins, new(3, [], [new(items.Gold, 100)], QuestType.Recurring) },
        { quests.FarmTrents, new(3, [], [new(items.Log, 1)], QuestType.Recurring) },
        { quests.FarmGolems, new(3, [], [new(items.IronOre, 1)], QuestType.Recurring) },
        { quests.FarmBats, new(3, [], [new(items.Gold, 150)], QuestType.Recurring) },
        { quests.FarmSpiders, new(3, [], [new(items.Web, 1)], QuestType.Recurring) },
        { quests.FarmSlimes, new(3, [], [new(items.Slime, 1)], QuestType.Recurring) },
        { quests.MineIronOre, new(3, [], [new(items.IronOre, 5)], QuestType.Recurring) },
    };
}

public static class QuestDetailsBuilder
{
    

}