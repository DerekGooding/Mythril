namespace Mythril.Data;

public readonly partial record struct Stat;

[Singleton]
public partial class Stats : IContent<Stat>
{
    public Stat[] All { get; private set; } = [ new("Placeholder", "Dummy") ];

    public void Load(IEnumerable<Stat> data)
    {
        All = [.. data];
    }
}
