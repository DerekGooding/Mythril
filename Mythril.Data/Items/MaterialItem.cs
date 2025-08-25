using Newtonsoft.Json;

namespace Mythril.Data.Items;

public class MaterialItem : Item
{
    // Properties specific to consumable items can go here
    // For example, what effect does it have when used?

    [JsonConstructor]
    public MaterialItem(string name, string description, int value)
        : base(name, description, value, ItemType.Material)
    {
    }

    public MaterialItem() { }
}
