using System.Collections.Immutable;

namespace Mythril.Data;

public record GameState(
    ImmutableDictionary<string, int> Inventory,
    int MagicCapacity,
    ImmutableHashSet<string> PinnedItems,
    ImmutableDictionary<string, string?> AssignedCadences, // CadenceName -> CharacterName?
    ImmutableList<Junction> Junctions,
    ImmutableDictionary<string, ImmutableDictionary<string, int>> CharacterPermanentStatBoosts,
    ImmutableHashSet<string> CompletedQuests,
    ImmutableHashSet<string> UnlockedAbilities,
    ImmutableList<QuestProgress> ActiveQuests,
    ImmutableDictionary<string, bool> AutoQuestEnabled,
    ImmutableDictionary<string, string?> LastFinishedActivity, // CharacterName -> TaskName
    ImmutableHashSet<string> StarredRecipes,
    ImmutableHashSet<string> UnlockedLocationNames,
    ImmutableHashSet<string> UnlockedCadenceNames,
    ImmutableHashSet<string> HighlightedPath,
    ImmutableHashSet<string> EverPerformedActivities,
    ImmutableHashSet<string> SeenContent,
    double CurrentTime,
    bool IsTestMode,
    bool HasUnseenCadence,
    bool HasUnseenWorkshop,
    string ActiveTab
)
{
    public static GameState Initial => new(
        Inventory: [],
        MagicCapacity: 30,
        PinnedItems: [],
        AssignedCadences: [],
        Junctions: [],
        CharacterPermanentStatBoosts: [],
        CompletedQuests: [],
        UnlockedAbilities: [],
        ActiveQuests: [],
        AutoQuestEnabled: [],
        LastFinishedActivity: [],
        StarredRecipes: [],
        UnlockedLocationNames: [],
        UnlockedCadenceNames: [],
        HighlightedPath: [],
        EverPerformedActivities: [],
        SeenContent: [],
        CurrentTime: 0,
        IsTestMode: false,
        HasUnseenCadence: false,
        HasUnseenWorkshop: false,
        ActiveTab: "hand"
    );
}

public static class NamedExtensions
{
    public static Dictionary<string, T> ToNamedDictionary<T>(this IEnumerable<T> source) where T : INamed
        => source.ToDictionary(x => x.Name);

    public static Dictionary<string, TValue> ToNamedDictionary<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where T : INamed
        => source.ToDictionary(x => x.Name, valueSelector);
}