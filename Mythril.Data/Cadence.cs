namespace Mythril.Data;

public readonly record struct Cadence(string Name, string Description, CadenceAbility[] Abilities) : INamed;

[Singleton]
public partial class Cadences(CadenceAbilities abilities) : IContent<Cadence>
{
    public Cadence[] All { get; } =
    [
        new ("Recruit", "Foundational adventuring cadence", [ abilities.AutoQuestI, abilities.AugmentStrength]),
        new ("Tinker", "Has the foundational item refining abilities", [ abilities.AutoQuestI]),
        new ("Acolyte", "Has the foundational magic refining abilities", [ abilities.AutoQuestI, abilities.RefineFire]),
    ];
}
