namespace Mythril.Data;

[Unique] public partial record struct Quest;

[Singleton]
public partial class Quests : IContent<Quest>
{
    public Quest[] All { get; } =
    [
        new ("Prologue", "Watch the intro cinematic."),
        new ("Tutorial Section", "Complete the tutorial section of the game."),
        new ("Visit Starting Town", "Time to see if there is anything useful here."),
        new ("Buy Potion", "Get a potion from the town shop."),
        new ("Learn About Cadences", "The old man teaches you how to unlock your true potential."),

        new ("Farm Goblins", "They got gold and we need it!"),
        new ("Farm Trents", "If I had the chance, I wood!"),
        new ("Farm Golems", "Either Ore!"),

        new ("Learn about the Mines", ""),
        new ("Farm Bats", ""),
        new ("Farm Spiders", ""),
        new ("Farm Slimes", ""),
        new ("Unlock Mining", "The blacksmith teaches you how to mine."),
        new ("Mine Iron Ore", ""),
    ];
}
