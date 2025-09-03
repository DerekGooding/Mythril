namespace Mythril.Data;

[Unique]
public readonly partial record struct CadenceAbility(string Name, string Description, Dictionary<string, int> Requirements) : INamed;


[Singleton]
public partial class CadenceAbilities(Items items) : IContent<CadenceAbility>
{
    public CadenceAbility[] All { get; } =
    [
        new ("AutoQuest I", "Allows a Quest to be looped indefinitely", []),
        new ("Refine Fire", "Refine items into fire magic",
            new Dictionary<string, int>() { { items.Gold.Name, 1000} }),

        new ("Refine Wood", "The first crafting refinement. Turns materials into other materials or consumables.",
            new Dictionary<string, int>() { { items.Gold.Name, 1000} }),

        new("Refine Mixology", "Allows the creation of potions and elixirs from herbs and other ingredients.",
            new Dictionary<string, int>() { { items.Gold.Name, 1000} }),

        new ("Augment Strength", "Augment strength stat with magic" ,
            new Dictionary<string, int>() { { items.Gold.Name, 1000} }),

        new ("Augment Health", "Augment health stat with magic", []),
        new ("Augment Vitality", "Augment vitality stat with magic", []),
        new ("Augment Speed", "Augment speed stat with magic", []),
        new ("Augment Magic", "Augment magic stat with magic", []),
        new ("Augment Spirit", "Augment spirit stat with magic", []),
        new ("Augment Luck", "Augment luck stat with magic", []),
        new ("Augment Hit", "Augment hit stat with magic", []),
        new ("Augment Evade", "Augment evade stat with magic", []),
    ];
}
