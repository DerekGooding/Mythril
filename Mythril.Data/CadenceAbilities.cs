namespace Mythril.Data;

public record struct CadenceAbility(string Name, string Description) : INamed
{
    public Dictionary<string, int> Requirements { get; set; } = [];
}


[Singleton]
public partial class CadenceAbilities(Items items) : IContent<CadenceAbility>
{
    public CadenceAbility[] All { get; } =
    [
        new ("AutoQuest I", "Allows a Quest to be looped indefinitely"),
        new ("Refine Fire", "Refine items into fire magic")
        {
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} }
        },
        new ("Augment Strength", "Augment strength stat with magic")
        {
            Requirements = new Dictionary<string, int>() { { items.Gold.Name, 1000} }
        },
    ];
}