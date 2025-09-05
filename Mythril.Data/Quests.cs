namespace Mythril.Data;

[Unique] public partial record struct Quest(string Name, string Description, int DurationSeconds, ItemQuantity[] Requirements, ItemQuantity[] Rewards,  bool SingleUse = false) : INamed;

[Singleton]
public partial class Quests(Items items) : IContent<Quest>
{
    public Quest[] All { get; } =
    [
        new Quest("Prologue", "Watch the intro cinematic.", 3, [], [], true),
        new Quest("Tutorial Section", "Complete the tutorial section of the game.", 3, [], [new ItemQuantity(items.BasicGem)]),
        new Quest("Visit Starting Town", "Time to see if there is anything useful here.", 3,[],[], true),
        new Quest("Buy Potion", "Get a potion from the town shop.", 3,
            [new ItemQuantity(items.Gold, 250)],
            [new ItemQuantity(items.Potion)]),
        new Quest("Learn About Cadences", "The old man teaches you how to unlock your true potential.", 3, [],[],true),
        new Quest("Farm Goblins", "They got gold and we need it!", 3, [],
        [new ItemQuantity(items.Gold, 100)]),
    ];
}
