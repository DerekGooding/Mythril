namespace Mythril.Data;

public record struct Cadence(string Name, string Description, List<string> Abilities) : INamed;

[Singleton]
public partial class Cadences : IContent<Cadence>
{
    public Cadence[] All { get; } =
    [
        new("Recruit", "Foundational adventuring cadence", [])
    ];
}