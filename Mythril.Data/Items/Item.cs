using Newtonsoft.Json;

namespace Mythril.Data.Items;

public enum ItemType
{
    Consumable,
    Equipment,
    Material,
    KeyItem
}

public abstract class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Value { get; set; } // Sell price
    public ItemType Type { get; set; }

    [JsonConstructor]
    protected Item(string name, string description, int value, ItemType type)
    {
        Name = name;
        Description = description;
        Value = value;
        Type = type;
    }

    protected Item()
    {
        Name = string.Empty;
        Description = string.Empty;
    }
}
