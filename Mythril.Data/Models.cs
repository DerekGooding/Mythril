using System.Text.Json.Serialization;

namespace Mythril.Data;

// Quest types
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestType
{
    Single,
    Recurring,
    Unlock
}

public partial record struct Quest(string Name, string Description) : INamed;

// Item types
[JsonConverter(typeof(JsonStringEnumConverter))]
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
public partial record struct CadenceAbility(string Name, string Description) : INamed
{
    public Dictionary<string, string> Metadata { get; init; } = [];

    public bool Equals(CadenceAbility other)
    {
        if (Name != other.Name || Description != other.Description) return false;
        var thisMeta = Metadata ?? [];
        var otherMeta = other.Metadata ?? [];
        if (thisMeta.Count != otherMeta.Count) return false;
        foreach (var kvp in thisMeta)
        {
            if (!otherMeta.TryGetValue(kvp.Key, out var val) || val != kvp.Value) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Description);
        var thisMeta = Metadata ?? [];
        foreach (var kvp in thisMeta.OrderBy(x => x.Key))
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }
}

public readonly record struct CadenceUnlock(string CadenceName, CadenceAbility Ability, ItemQuantity[] Requirements, string PrimaryStat = "Magic");

public partial record struct Cadence(string Name, string Description, CadenceUnlock[] Abilities) : INamed;

// Location types
public partial record struct Location(string Name, IEnumerable<Quest> Quests, string? RequiredQuest = null) : INamed;

// Character
public partial record struct Character(string Name);

// Stats
public partial record struct Stat(string Name, string Description) : INamed;

public readonly record struct StatAugment(Stat Stat, int ModifierAtFull);

// Details
public readonly record struct QuestDetail(int DurationSeconds, ItemQuantity[] Requirements, ItemQuantity[] Rewards, QuestType Type, string PrimaryStat = "Vitality", Dictionary<string, int>? RequiredStats = null, Dictionary<string, int>? StatRewards = null);

// Refinements
public readonly record struct Recipe(int InputQuantity, Item OutputItem, int OutputQuantity);

public readonly record struct RefinementData(CadenceAbility Ability, Item InputItem, Recipe Recipe, string PrimaryStat = "Strength")
{
    public string Name => $"{Ability.Name}: {Recipe.OutputItem.Name}";
    public string Description => $"Refine {Recipe.InputQuantity}x {InputItem.Name} into {Recipe.OutputQuantity}x {Recipe.OutputItem.Name}";
}

// Persistence
public class SaveData
{
    public List<KeyValuePair<string, int>> Inventory { get; set; } = [];
    public List<string> UnlockedCadences { get; set; } = [];
    public List<string> UnlockedAbilities { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];
    public List<QuestProgressDTO> ActiveQuests { get; set; } = [];
    public List<JournalEntryDTO> Journal { get; set; } = [];
    public List<JunctionDTO> Junctions { get; set; } = [];
    public List<AssignedCadenceDTO> AssignedCadences { get; set; } = [];
    public Dictionary<string, bool> AutoQuestEnabled { get; set; } = [];
    public List<string> UnlockedLocations { get; set; } = []; // Added UnlockedLocations
    public bool HasUnseenCadence { get; set; }
    public bool HasUnseenWorkshop { get; set; }
    public DateTime LastSaveTime { get; set; }
    public Dictionary<string, Dictionary<string, int>> CharacterStatBoosts { get; set; } = [];
}

public class AssignedCadenceDTO
{
    public string CadenceName { get; set; } = "";
    public string CharacterName { get; set; } = "";
}

public record Junction(Character Character, Stat Stat, Item Magic);

public class JunctionDTO
{
    public string CharacterName { get; set; } = "";
    public string StatName { get; set; } = "";
    public string MagicName { get; set; } = "";
}

public class QuestProgressDTO
{
    public string ItemName { get; set; } = string.Empty;
    public string AbilityName { get; set; } = string.Empty;
    public string InputItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // "Quest", "CadenceUnlock", "Refinement"
    public string CharacterName { get; set; } = string.Empty;
    public double SecondsElapsed { get; set; }
    public DateTime StartTime { get; set; }
    public int SlotIndex { get; set; }
}

public class JournalEntryDTO
{
    public string TaskName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime CompletedAt { get; set; }
    public bool IsFirstTime { get; set; }
}

// Data Transfer Objects for JSON Loading
public class ItemQuantityDTO { public string Item { get; set; } = ""; public int Quantity { get; set; } = 1; }
public class LocationDTO { public string Name { get; set; } = ""; public List<string> Quests { get; set; } = []; public string? RequiredQuest { get; set; } }
public class CadenceAbilityUnlockDTO 
{ 
    public string Ability { get; set; } = ""; 
    public List<ItemQuantityDTO> Requirements { get; set; } = []; 
    public string PrimaryStat { get; set; } = "Magic";
    public Dictionary<string, string> Metadata { get; set; } = [];
}
public class CadenceDTO { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public List<CadenceAbilityUnlockDTO> Abilities { get; set; } = []; }
public class QuestDetailDTO { public string Quest { get; set; } = ""; public int DurationSeconds { get; set; } = 3; public string Type { get; set; } = "Single"; public List<ItemQuantityDTO> Requirements { get; set; } = []; public List<ItemQuantityDTO> Rewards { get; set; } = []; public string PrimaryStat { get; set; } = "Vitality"; public Dictionary<string, int>? RequiredStats { get; set; } public Dictionary<string, int>? StatRewards { get; set; } }
public class QuestUnlockDTO { public string Quest { get; set; } = ""; public List<string> Requires { get; set; } = []; }
public class QuestCadenceUnlockDTO { public string Quest { get; set; } = ""; public List<string> Cadences { get; set; } = []; }
public class RecipeDTO { public string InputItem { get; set; } = ""; public int InputQuantity { get; set; } = 1; public string OutputItem { get; set; } = ""; public int OutputQuantity { get; set; } = 1; }
public class RefinementDTO { public string Ability { get; set; } = ""; public List<RecipeDTO> Recipes { get; set; } = []; public string PrimaryStat { get; set; } = "Strength"; }
public class StatAugmentEntryDTO { public string Stat { get; set; } = ""; public int ModifierAtFull { get; set; } = 0; }
public class StatAugmentItemDTO { public string Item { get; set; } = ""; public List<StatAugmentEntryDTO> Augments { get; set; } = []; }
