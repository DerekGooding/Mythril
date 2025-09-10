namespace Mythril.Data;

public enum QuestType
{
    Single,
    Recurring,
    Unlock
}

[Unique] public partial record struct Quest(string Name,
                                            string Description,
                                            int DurationSeconds,
                                            ItemQuantity[] Requirements,
                                            ItemQuantity[] Rewards,
                                            QuestType Type) : INamed;

[Singleton]
public partial class Quests(Items items) : IContent<Quest>
{
    public Quest[] All { get; } =
    [
        new Quest("Prologue", "Watch the intro cinematic.", 3, [], [], QuestType.Single),
        new Quest("Tutorial Section", "Complete the tutorial section of the game.", 3, [], [new ItemQuantity(items.BasicGem)], QuestType.Recurring),
        new Quest("Visit Starting Town", "Time to see if there is anything useful here.", 3,[],[], QuestType.Single),
        new Quest("Buy Potion", "Get a potion from the town shop.", 3,
            [new ItemQuantity(items.Gold, 250)],
            [new ItemQuantity(items.Potion)], QuestType.Recurring),
        new Quest("Learn About Cadences", "The old man teaches you how to unlock your true potential.", 3, [],[],QuestType.Single),
        new Quest("Farm Goblins", "They got gold and we need it!", 3, [],
        [new ItemQuantity(items.Gold, 100)], QuestType.Recurring),
        new Quest("Farm Trents", "If I had the chance, I wood!", 3, [],
        [new ItemQuantity(items.Log, 1)], QuestType.Recurring),
        new Quest("Farm Golems", "Either Ore!", 3, [],
        [new ItemQuantity(items.IronOre, 1)], QuestType.Recurring),
    ];
}
