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
    public List<QuestProgressDTO> ActiveQuests { get; set; } = [];
}

public class QuestProgressDTO
{
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // "Quest" or "CadenceUnlock"
    public string CharacterName { get; set; } = string.Empty;
    public int SecondsElapsed { get; set; }
}

// Data Transfer Objects for JSON Loading
public class ItemQuantityDTO { public string Item { get; set; } = ""; public int Quantity { get; set; } = 1; }
public class LocationDTO { public string Name { get; set; } = ""; public List<string> Quests { get; set; } = []; }
public class CadenceAbilityUnlockDTO { public string Ability { get; set; } = ""; public List<ItemQuantityDTO> Requirements { get; set; } = []; }
public class CadenceDTO { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public List<CadenceAbilityUnlockDTO> Abilities { get; set; } = []; }
public class QuestDetailDTO { public string Quest { get; set; } = ""; public int DurationSeconds { get; set; } = 3; public string Type { get; set; } = "Single"; public List<ItemQuantityDTO> Requirements { get; set; } = []; public List<ItemQuantityDTO> Rewards { get; set; } = []; }
public class QuestUnlockDTO { public string Quest { get; set; } = ""; public List<string> Requires { get; set; } = []; }
public class RecipeDTO { public string InputItem { get; set; } = ""; public int InputQuantity { get; set; } = 1; public string OutputItem { get; set; } = ""; public int OutputQuantity { get; set; } = 1; }
public class RefinementDTO { public string Ability { get; set; } = ""; public List<RecipeDTO> Recipes { get; set; } = []; }
