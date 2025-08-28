namespace Mythril.Data;

public record struct Item(string Name, string Description) : INamed
{
    public int Quantity { get; set; } = 0;
}

[Singleton]
public partial class Items : IContent<Item>
{
    public Item[] All { get; } =
    [
        new("Gold", "The currency of the realm."),
        new("Potion", "Restores a small amount of health."),
        new("Basic Gem", "Is refined into the primary elemental magic.")
    ];
}