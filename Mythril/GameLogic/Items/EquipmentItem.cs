using Newtonsoft.Json;

namespace Mythril.GameLogic.Items;

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}

public class EquipmentItem : Item
{
    public EquipmentSlot Slot { get; set; }
    // Properties specific to equipment can go here
    // For example, stat bonuses

    [JsonConstructor]
    public EquipmentItem(string name, string description, int value, EquipmentSlot slot)
        : base(name, description, value, ItemType.Equipment) => Slot = slot;

    public EquipmentItem() : base() { }
}
