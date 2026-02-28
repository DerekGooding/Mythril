namespace Mythril.Data;

[Unique] public readonly partial record struct Item;

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
        new("Log", "A basic piece of wood.", ItemType.Material),
        new("Iron Ore", "A common ore used in crafting.", ItemType.Material),
        new("Herb", "A medicinal plant used in potions.", ItemType.Material),
        new("Leather", "Tough animal hide used in crafting.", ItemType.Material),
        new("Water", "Essential for life and potion-making.", ItemType.Material),
        new("Web", "Sticky silk produced by spiders.", ItemType.Material),
        new("Slime", "A gelatinous creature's residue.", ItemType.Material),

        //Spells
        new("Fire I", "A basic fire spell.", ItemType.Spell),
        new("Ice I", "A basic ice spell.", ItemType.Spell),
        new("Lightning I", "A basic lightning spell.", ItemType.Spell),
        new("Earth I", "A basic earth spell.", ItemType.Spell),
    ];
}
