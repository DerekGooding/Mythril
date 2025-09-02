namespace Mythril.Data;

public enum ItemType
{
    Currency,
    Consumable,
    Material,
    Spell,
}

public record struct Item(string Name, string Description, ItemType ItemType) : INamed
{
    public int Quantity { get; set; }
}

[Singleton]
public partial class Items : IContent<Item>
{
    public Item[] All { get; } =
    [
        new("Gold", "The currency of the realm.", ItemType.Currency),

        //Consumables
        new("Potion", "Restores a small amount of health.", ItemType.Consumable),

        //Materials
        new("Basic Gem", "Is refined into the primary elemental magic.", ItemType.Material),

        //Spells
        new("Fire I", "A basic fire spell.", ItemType.Spell),
    ];
}