namespace Mythril.Data;

public readonly partial record struct Item;

[Singleton]
public partial class Items : IContent<Item>
{
    public Item[] All { get; private set; } = [ new("Placeholder", "Dummy", ItemType.Material) ];

    public void Load(IEnumerable<Item> data)
    {
        All = [.. data];
    }
}
