namespace Mythril.Data;

public readonly record struct Cadence(string Name, string Description, string[] Abilities) : INamed;

[Singleton]
public partial class Cadences : IContent<Cadence>
{
    public Cadence[] All { get; } =
    [
        new("Recruit", "Foundational adventuring cadence", [])
    ];
}