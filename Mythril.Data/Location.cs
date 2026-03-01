namespace Mythril.Data;

public readonly partial record struct Location;

[Singleton]
public partial class Locations : IContent<Location>
{
    public Location[] All { get; private set; } = [ new("Placeholder", []) ];

    public void Load(IEnumerable<Location> data)
    {
        All = [.. data];
    }
}
