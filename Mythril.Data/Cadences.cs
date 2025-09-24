using static Mythril.Data.CadenceBuilder;

namespace Mythril.Data;

public readonly record struct CadenceUnlock(CadenceAbility Ability, ItemQuantity[] Requirements);

[Unique] public readonly partial record struct Cadence(string Name, string Description, CadenceUnlock[] Abilities) : INamed;

[Singleton]
public partial class Cadences(CadenceAbilities abilities, Items items) : IContent<Cadence>
{
    public Cadence[] All { get; } =
    [
        new ("Recruit", "Foundational adventuring cadence",
        [
            new (abilities.AutoQuestI,      Requirements((items.Gold, 100))),
            new (abilities.AugmentStrength, Requirements((items.IronOre, 10))),
        ]),
        new ("Apprentice", "Has the foundational item refining abilities",
        [
            new(abilities.AutoQuestI,       Requirements((items.Gold, 100))),
            new(abilities.RefineScrap,      Requirements((items.Gold, 1000))),
            new(abilities.RefineMixology,   Requirements((items.Herb, 30), (items.IronOre, 10))),
        ]),
        new ("Student", "Has the foundational magic refining abilities",
        [
            new(abilities.AutoQuestI,       Requirements((items.Gold, 100))),
            new(abilities.RefineFire,       Requirements((items.Log, 10))),
            new(abilities.RefineIce,        Requirements((items.Potion, 10))),
        ]),
    ];
}

public static class CadenceBuilder
{
    public static ItemQuantity[] Requirements(params (Item Item, int Amount)[] req) => [.. req.Select(x => new ItemQuantity(x.Item, x.Amount))];

    private class Builder;
}