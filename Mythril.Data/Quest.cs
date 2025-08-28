namespace Mythril.Data;

public record struct Quest(string Name, string Description, int DurationSeconds) : INamed
{
    public bool SingleUse { get; set; }

    public Dictionary<string, int> Requirements { get; set; } = [];
    public Dictionary<string, int> Rewards { get; set; } = [];
}

[Singleton]
public partial class Quests(Items items) : IContent<Quest>
{
    public Quest[] All { get; } =
    [
        new( "Prologue", "Watch the intro cinematic.", 3)
        {
            SingleUse = true
        },
        new("Tutorial Section", "Complete the tutorial section of the game.", 3)
        {
            Rewards = new Dictionary<string, int>() { { items.BasicGem.Name, 1 } },
        },
        new("Visit Starting Town", "Time to see if there is anything useful here.", 3)
        {
            SingleUse = true,
        },
        new("Buy Potion", "Get a potion from the town shop.", 3)
        {
            Rewards = new Dictionary<string, int>() { { items.Potion.Name, 1 } },
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 250} },
        },
        new("Unlock Strength Junction", "Purchase Strength Junctioning from the Old Man.", 3)
        {
            SingleUse = true,
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} },
        },
        new("Unlock Fire Refine Ability", "Purchase the Fire Refinement ability from the Old Man.", 3)
        {
            SingleUse = true,
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} },
        },
        new("Farm Goblins", "They got gold and we need it!", 3)
        {
            Rewards = new Dictionary<string, int>() { { items.Gold.Name, 100 } },
        },
    ];
}
