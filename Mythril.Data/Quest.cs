namespace Mythril.Data;

public class Quest(string name) : INamed
{
    public string Name { get; set; } = name;
    public string Title { get; set; } = name;
    public string? Description { get; set; }
    public int DurationSeconds { get; set; }

    public bool SingleUse { get; set; } = false;

    public Dictionary<string, int> Requirements { get; set; } = [];
    public Dictionary<string, int> Rewards { get; set; } = [];

    public List<string> Prerequisites { get; set; } = [];
}

[Singleton]
public partial class Quests(Items items) : IContent<Quest>
{
    public Quest[] All { get; } =
    [
        new( "Prologue")
        {
            Description = "Watch the intro cinematic.",
            DurationSeconds = 3,
            SingleUse = true
        },
        new("Tutorial Section")
        {
            Description = "Complete the tutorial section of the game.",
            DurationSeconds = 3,
            Requirements = new Dictionary<string, int>() { { items.Potion.Name, 1 } },
        },
        new("Visit Starting Town")
        {
            Description = "Time to see if there is anything useful here.",
            DurationSeconds = 3,
            SingleUse = true,
        },
        new("Buy Potion")
        {
            Description = "Get a potion from the town shop.",
            DurationSeconds = 3,
            Rewards = new Dictionary<string, int>() { { items.Potion.Name, 1 } },
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 250} },
        },
        new("Unlock Strength Junction")
        {
            Description = "Purchase Strength Junctioning from the Old Man.",
            DurationSeconds = 3,
            SingleUse = true,
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} },
        },
        new("Unlock Fire Refine Ability")
        {
            Description = "Purchase the Fire Refinement ability from the Old Man.",
            DurationSeconds = 3,
            SingleUse = true,
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} },
        },
        new("Farm Goblins")
        {
            Description = "They got gold and we need it!",
            DurationSeconds = 3,
            Rewards = new Dictionary<string, int>() { { items.Gold.Name, 100 } },
        },
    ];
}
