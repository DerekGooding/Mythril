namespace Mythril.Data;

// Quest types
public enum QuestType
{
    Single,
    Recurring,
    Unlock
}

public partial record struct Quest(string Name, string Description) : INamed;

// Item types
public enum ItemType
{
    Currency,
    Consumable,
    Material,
    Spell,
}

public partial record struct Item(string Name, string Description, ItemType ItemType) : INamed;

public readonly record struct ItemQuantity(Item Item, int Quantity = 1);

// Cadence types
public partial record struct CadenceAbility(string Name, string Description) : INamed;

public readonly record struct CadenceUnlock(CadenceAbility Ability, ItemQuantity[] Requirements);

public partial record struct Cadence(string Name, string Description, CadenceUnlock[] Abilities) : INamed;

// Location types
public partial record struct Location(string Name, IEnumerable<Quest> Quests) : INamed;

// Character
public partial record struct Character(string Name);

// Stats
public partial record struct Stat(string Name, string Description) : INamed;

public readonly record struct StatAugment(Stat Stat, int ModifierAtFull);

// Details
public readonly record struct QuestDetail(int DurationSeconds, ItemQuantity[] Requirements, ItemQuantity[] Rewards, QuestType Type);

// Refinements
public readonly record struct Recipe(int InputQuantity, Item OutputItem, int OutputQuantity);

// Persistence
public class SaveData
{
    public List<KeyValuePair<string, int>> Inventory { get; set; } = [];
    public List<string> UnlockedCadences { get; set; } = [];
    public List<string> UnlockedAbilities { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];
}
