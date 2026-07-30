using System.Text.Json;
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
    KeyItem,
}

public partial record struct Item(string Name, string Description, ItemType ItemType) : INamed
{
    public string Category => ItemType switch
    {
        ItemType.Spell => "Magic",
        ItemType.KeyItem => "Key Items",
        _ => "Materials"
    };
}

public readonly record struct ItemQuantity(Item Item, int Quantity = 1);

// Cadence types
// Effect types
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EffectType
{
    MagicCapacity,
    AutoQuest,
    Logistics,
    StatBoost,
}

public record EffectDefinition(EffectType Type, int Value, string? Target = null);

public partial record struct CadenceAbility(string Name, string Description) : INamed
{
    public Dictionary<string, string> Metadata { get; init; } = [];
    public EffectDefinition[] Effects { get; init; } = [];

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

        var thisEffects = Effects ?? [];
        var otherEffects = other.Effects ?? [];
        if (thisEffects.Length != otherEffects.Length) return false;
        for (var i = 0; i < thisEffects.Length; i++)
        {
            if (thisEffects[i] != otherEffects[i]) return false;
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

        var thisEffects = Effects ?? [];
        foreach (var effect in thisEffects)
        {
            hash.Add(effect);
        }

        return hash.ToHashCode();
    }
}

public readonly record struct CadenceUnlock(string CadenceName, CadenceAbility Ability, ItemQuantity[] Requirements, string PrimaryStat = "Magic");

public partial record struct Cadence(string Name, string Description, CadenceUnlock[] Abilities) : INamed;

// Location types
public partial record struct Location(string Name, IEnumerable<Quest> Quests, string? RequiredQuest = null, string? Type = null) : INamed;

// Character
public partial record struct Character(string Name)
{
    public string Color => Name switch
    {
        "Protagonist" => "#ff4444",
        "Wifu" => "#4444ff",
        "Himbo" => "#44ff44",
        _ => "#00adb5"
    };

    public string Icon => Name switch
    {
        "Protagonist" => "person",
        "Wifu" => "face",
        "Himbo" => "sentiment_satisfied_alt",
        _ => "account_circle"
    };
}

// Stats
public partial record struct Stat(string Name, string Description) : INamed;

public readonly record struct StatAugment(Stat Stat, int ModifierAtFull);

// Details
public readonly record struct QuestDetail(int DurationSeconds, ItemQuantity[] Requirements, ItemQuantity[] Rewards, QuestType Type, string PrimaryStat = "Vitality", Dictionary<string, int>? RequiredStats = null, Dictionary<string, int>? StatRewards = null, EffectDefinition[]? Effects = null);

// Refinements
public readonly record struct Recipe(int InputQuantity, Item OutputItem, int OutputQuantity, ItemQuantity[]? AdditionalInputs = null);

public readonly record struct RefinementData(CadenceAbility Ability, Item InputItem, Recipe Recipe, string PrimaryStat = "Strength")
{
    public string Name => $"{Ability.Name} ({InputItem.Name}): {Recipe.OutputItem.Name}";
    public string Description => Recipe.AdditionalInputs != null && Recipe.AdditionalInputs.Length > 0
        ? $"Refine {Recipe.InputQuantity}x {InputItem.Name} + {string.Join(", ", Recipe.AdditionalInputs.Select(i => $"{i.Quantity}x {i.Item.Name}"))} into {Recipe.OutputQuantity}x {Recipe.OutputItem.Name}"
        : $"Refine {Recipe.InputQuantity}x {InputItem.Name} into {Recipe.OutputQuantity}x {Recipe.OutputItem.Name}";
}

// Unified Content Graph
public class FlexibleInEdgesConverter : JsonConverter<Dictionary<string, List<string>>>
{
    public override Dictionary<string, List<string>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new Dictionary<string, List<string>>();
        if (reader.TokenType != JsonTokenType.StartObject)
            return result;

        using var doc = JsonDocument.ParseValue(ref reader);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            var list = new List<string>();
            if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.Value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        var str = item.GetString();
                        if (str != null) list.Add(str);
                    }
                    else if (item.ValueKind == JsonValueKind.Object)
                    {
                        if (item.TryGetProperty("targetId", out var targetProp) && targetProp.ValueKind == JsonValueKind.String)
                        {
                            var targetId = targetProp.GetString();
                            if (targetId != null) list.Add(targetId);
                        }
                    }
                }
            }
            result[prop.Name] = list;
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, List<string>> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class ContentNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = ""; // Quest, Location, Cadence, Item, Refinement

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = [];

    [JsonPropertyName("in_edges")]
    [JsonConverter(typeof(FlexibleInEdgesConverter))]
    public Dictionary<string, List<string>> InEdges { get; set; } = []; // RelationType -> [NodeIds]

    [JsonPropertyName("out_edges")]
    public Dictionary<string, List<ContentEdge>> OutEdges { get; set; } = []; // RelationType -> [Edges]

    [JsonPropertyName("effects")]
    public List<EffectDefinition>? Effects { get; set; }
}

public class ContentEdge
{
    [JsonPropertyName("targetId")]
    public string TargetId { get; set; } = "";

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}

// Persistence
public class SaveData
{
    public Dictionary<string, int> Inventory { get; set; } = [];
    public int MagicCapacity { get; set; } = 30;
    public List<string> PinnedItems { get; set; } = [];
    public List<string> UnlockedCadences { get; set; } = [];
    public List<string> UnlockedAbilities { get; set; } = [];
    public List<string> CompletedQuests { get; set; } = [];
    public List<QuestProgressDTO> ActiveQuests { get; set; } = [];
    public List<JunctionDTO> Junctions { get; set; } = [];
    public List<AssignedCadenceDTO> AssignedCadences { get; set; } = [];
    public Dictionary<string, bool> AutoQuestEnabled { get; set; } = [];
    public List<string> UnlockedLocations { get; set; } = [];
    public List<string> StarredRecipes { get; set; } = [];
    public List<string> SeenContent { get; set; } = [];
    public bool HasUnseenCadence { get; set; }
    public bool HasUnseenWorkshop { get; set; }
    public double CurrentTime { get; set; }
    public bool IsTestMode { get; set; }
    public string ActiveTab { get; set; } = "hand";
    public DateTime LastSaveTime { get; set; }
    public Dictionary<string, Dictionary<string, int>> CharacterStatBoosts { get; set; } = [];
    public Dictionary<string, string?> LastFinishedActivities { get; set; } = [];
    public List<string> EverPerformedActivities = [];
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
    public string Description { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
}

// Data Transfer Objects for JSON Loading
public class ItemQuantityDTO
{ public string Item { get; set; } = ""; public int Quantity { get; set; } = 1; }

public class LocationDTO
{ public string Name { get; set; } = ""; public List<string> Quests { get; set; } = []; public string? RequiredQuest { get; set; } public string? Type { get; set; } }

public class CadenceAbilityUnlockDTO
{
    public string Ability { get; set; } = "";
    public List<ItemQuantityDTO> Requirements { get; set; } = [];
    public string PrimaryStat { get; set; } = "Magic";
    public Dictionary<string, string> Metadata { get; set; } = [];
    public List<EffectDefinition> Effects { get; set; } = [];
}

public class CadenceDTO
{ public string Name { get; set; } = ""; public string Description { get; set; } = ""; public List<CadenceAbilityUnlockDTO> Abilities { get; set; } = []; }

public class QuestDetailDTO
{ public string Quest { get; set; } = ""; public int DurationSeconds { get; set; } = 3; public string Type { get; set; } = "Single"; public List<ItemQuantityDTO> Requirements { get; set; } = []; public List<ItemQuantityDTO> Rewards { get; set; } = []; public string PrimaryStat { get; set; } = "Vitality"; public Dictionary<string, int>? RequiredStats { get; set; } public Dictionary<string, int>? StatRewards { get; set; } public List<EffectDefinition>? Effects { get; set; } }

public class QuestUnlockDTO
{ public string Quest { get; set; } = ""; public List<string> Requires { get; set; } = []; }

public class QuestCadenceUnlockDTO
{ public string Quest { get; set; } = ""; public List<string> Cadences { get; set; } = []; }

public class RecipeDTO
{ public string InputItem { get; set; } = ""; public int InputQuantity { get; set; } = 1; public string OutputItem { get; set; } = ""; public int OutputQuantity { get; set; } = 1; }

public class RefinementDTO
{ public string Ability { get; set; } = ""; public List<RecipeDTO> Recipes { get; set; } = []; public string PrimaryStat { get; set; } = "Strength"; }

public class StatAugmentEntryDTO
{ public string Stat { get; set; } = ""; public int ModifierAtFull { get; set; } = 0; }

public class StatAugmentItemDTO
{ public string Item { get; set; } = ""; public List<StatAugmentEntryDTO> Augments { get; set; } = []; }