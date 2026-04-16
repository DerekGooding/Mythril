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
    ImmutableHashSet<string> StarredRecipes,
    ImmutableHashSet<string> UnlockedLocationNames,
    ImmutableHashSet<string> UnlockedCadenceNames,
    ImmutableHashSet<string> HighlightedPath,
    ImmutableList<JournalEntry> Journal,
    ImmutableDictionary<string, ImmutableList<string>> CharacterMiniLogs,
    ImmutableHashSet<string> EverPerformedActivities,
    double CurrentTime,
    bool IsTestMode,
    bool HasUnseenCadence,
    bool HasUnseenWorkshop,
    string ActiveTab,
    bool ShowMiniLogs
)
{
    public static GameState Initial => new(
        Inventory: ImmutableDictionary<string, int>.Empty,
        MagicCapacity: 30,
        PinnedItems: ImmutableHashSet<string>.Empty,
        AssignedCadences: ImmutableDictionary<string, string?>.Empty,
        Junctions: ImmutableList<Junction>.Empty,
        CharacterPermanentStatBoosts: ImmutableDictionary<string, ImmutableDictionary<string, int>>.Empty,
        CompletedQuests: ImmutableHashSet<string>.Empty,
        UnlockedAbilities: ImmutableHashSet<string>.Empty,
        ActiveQuests: ImmutableList<QuestProgress>.Empty,
        AutoQuestEnabled: ImmutableDictionary<string, bool>.Empty,
        StarredRecipes: ImmutableHashSet<string>.Empty,
        UnlockedLocationNames: ImmutableHashSet<string>.Empty,
        UnlockedCadenceNames: ImmutableHashSet<string>.Empty,
        HighlightedPath: ImmutableHashSet<string>.Empty,
        Journal: ImmutableList<JournalEntry>.Empty,
        CharacterMiniLogs: ImmutableDictionary<string, ImmutableList<string>>.Empty,
        EverPerformedActivities: ImmutableHashSet<string>.Empty,
        CurrentTime: 0,
        IsTestMode: false,
        HasUnseenCadence: false,
        HasUnseenWorkshop: false,
        ActiveTab: "hand",
        ShowMiniLogs: false
    );
}

public static class NamedExtensions
{
    public static Dictionary<string, T> ToNamedDictionary<T>(this IEnumerable<T> source) where T : INamed
        => source.ToDictionary(x => x.Name);

    public static Dictionary<string, TValue> ToNamedDictionary<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where T : INamed
        => source.ToDictionary(x => x.Name, valueSelector);
}
