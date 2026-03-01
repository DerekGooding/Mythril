namespace Mythril.Data;

public readonly partial record struct Cadence;

[Singleton]
public partial class Cadences : IContent<Cadence>
{
    public Cadence[] All { get; private set; } = [ new("Placeholder", "Dummy", []) ];

    public void Load(IEnumerable<Cadence> data)
    {
        All = [.. data];
    }
}

public static class CadenceBuilder
{
    public static ItemQuantity[] Requirements(params (Item Item, int Amount)[] req) => [.. req.Select(x => new ItemQuantity(x.Item, x.Amount))];
}
