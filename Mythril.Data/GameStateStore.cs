using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    double CurrentTime
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
        CurrentTime: 0
    );
}

public interface IGameAction { }

public record AddResourceAction(string ItemName, int Quantity) : IGameAction;
public record SpendResourceAction(string ItemName, int Quantity) : IGameAction;
public record CompleteQuestAction(Quest Quest) : IGameAction;
public record LockQuestAction(Quest Quest) : IGameAction;
public record StartQuestAction(QuestProgress Progress) : IGameAction;
public record CancelQuestAction(QuestProgress Progress) : IGameAction;
public record AssignCadenceAction(string CadenceName, string CharacterName) : IGameAction;
public record UnassignCadenceAction(string CadenceName) : IGameAction;
public record JunctionMagicAction(Character Character, Stat Stat, Item Magic) : IGameAction;
public record UnjunctionAction(Character Character, Stat Stat) : IGameAction;
public record TickAction(double DeltaSeconds) : IGameAction;
public record UnlockAbilityAction(string AbilityKey) : IGameAction;
public record UnlockCadenceAction(string CadenceName) : IGameAction;
public record ToggleAutoQuestAction(string CharacterName, bool Enabled) : IGameAction;
public record TogglePinAction(string ItemName) : IGameAction;
public record ToggleRecipeStarAction(string RecipeKey) : IGameAction;
public record SetMagicCapacityAction(int Capacity) : IGameAction;
public record ClearInventoryAction() : IGameAction;
public record SetStateAction(GameState NewState) : IGameAction;
public record AddStatBoostAction(string CharacterName, string StatName, int Amount) : IGameAction;
public record UnlockLocationAction(string LocationName) : IGameAction;
public record SetHighlightedPathAction(ImmutableHashSet<string> Path) : IGameAction;
public record ClearHighlightedPathAction() : IGameAction;

public class GameStore
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GameState State { get; private set; } = GameState.Initial;
    public event Action<GameState>? OnStateChanged;
    public event Action<string, int>? OnItemOverflow;

    public string ExportState() => JsonSerializer.Serialize(State, _options);
    public void ImportState(string json)
    {
        try
        {
            var newState = JsonSerializer.Deserialize<GameState>(json, _options);
            if (newState != null) Dispatch(new SetStateAction(newState));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to restore state: " + ex.Message);
        }
    }

    public void Dispatch(IGameAction action)
    {
        State = Reduce(State, action, out var overflowItem, out var overflowQty);
        OnStateChanged?.Invoke(State);
        if (overflowItem != null && overflowQty > 0)
        {
            OnItemOverflow?.Invoke(overflowItem, overflowQty);
        }
    }

    private static GameState Reduce(GameState state, IGameAction action, out string? overflowItem, out int overflowQty)
    {
        overflowItem = null;
        overflowQty = 0;

        return action switch
        {
            AddResourceAction a => AddResource(state, a, out overflowItem, out overflowQty),
            SpendResourceAction a => SpendResource(state, a),
            CompleteQuestAction a => state with { CompletedQuests = state.CompletedQuests.Add(a.Quest.Name) },
            LockQuestAction a => state with { CompletedQuests = state.CompletedQuests.Remove(a.Quest.Name) },
            StartQuestAction a => state with { ActiveQuests = state.ActiveQuests.Add(a.Progress) },
            CancelQuestAction a => state with { ActiveQuests = state.ActiveQuests.Remove(a.Progress) },
            AssignCadenceAction a => state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, a.CharacterName) },
            UnassignCadenceAction a => UnassignCadence(state, a),
            JunctionMagicAction a => state with { 
                Junctions = state.Junctions.RemoveAll(j => j.Character.Name == a.Character.Name && j.Stat.Name == a.Stat.Name).Add(new Junction(a.Character, a.Stat, a.Magic)) 
            },
            UnjunctionAction a => state with {
                Junctions = state.Junctions.RemoveAll(j => j.Character.Name == a.Character.Name && j.Stat.Name == a.Stat.Name)
            },
            TickAction a => state with { 
                CurrentTime = state.CurrentTime + a.DeltaSeconds,
                ActiveQuests = state.ActiveQuests.Select(q => q.IsCompleted ? q : q with { SecondsElapsed = q.SecondsElapsed + a.DeltaSeconds }).ToImmutableList()
            },
            UnlockAbilityAction a => state with { UnlockedAbilities = state.UnlockedAbilities.Add(a.AbilityKey) },
            UnlockCadenceAction a => state with { UnlockedCadenceNames = state.UnlockedCadenceNames.Add(a.CadenceName) },
            ToggleAutoQuestAction a => state with { AutoQuestEnabled = state.AutoQuestEnabled.SetItem(a.CharacterName, a.Enabled) },
            TogglePinAction a => state with { PinnedItems = state.PinnedItems.Contains(a.ItemName) ? state.PinnedItems.Remove(a.ItemName) : state.PinnedItems.Add(a.ItemName) },
            ToggleRecipeStarAction a => state with { StarredRecipes = state.StarredRecipes.Contains(a.RecipeKey) ? state.StarredRecipes.Remove(a.RecipeKey) : state.StarredRecipes.Add(a.RecipeKey) },
            SetMagicCapacityAction a => state with { MagicCapacity = a.Capacity },
            ClearInventoryAction a => state with { Inventory = ImmutableDictionary<string, int>.Empty },
            SetStateAction a => a.NewState,
            AddStatBoostAction a => state with {
                CharacterPermanentStatBoosts = state.CharacterPermanentStatBoosts.SetItem(a.CharacterName, 
                    state.CharacterPermanentStatBoosts.GetValueOrDefault(a.CharacterName, ImmutableDictionary<string, int>.Empty)
                    .SetItem(a.StatName, state.CharacterPermanentStatBoosts.GetValueOrDefault(a.CharacterName, ImmutableDictionary<string, int>.Empty).GetValueOrDefault(a.StatName) + a.Amount))
            },
            UnlockLocationAction a => state with { UnlockedLocationNames = state.UnlockedLocationNames.Add(a.LocationName) },
            SetHighlightedPathAction a => state with { HighlightedPath = a.Path },
            ClearHighlightedPathAction a => state with { HighlightedPath = ImmutableHashSet<string>.Empty },
            _ => state
        };
    }

    private static GameState AddResource(GameState state, AddResourceAction a, out string? overflowItem, out int overflowQty)
    {
        overflowItem = null;
        overflowQty = 0;

        var items = ContentHost.GetContent<Items>();
        var item = items.All.FirstOrDefault(i => i.Name == a.ItemName);
        
        int current = state.Inventory.GetValueOrDefault(a.ItemName);
        int next = current + a.Quantity;

        if (item.Name != null && item.ItemType == ItemType.Spell && next > state.MagicCapacity)
        {
            overflowQty = next - state.MagicCapacity;
            overflowItem = a.ItemName;
            next = state.MagicCapacity;
        }

        return state with { Inventory = state.Inventory.SetItem(a.ItemName, next) };
    }

    private static GameState UnassignCadence(GameState state, UnassignCadenceAction a)
    {
        if (!state.AssignedCadences.TryGetValue(a.CadenceName, out var owner) || owner == null)
            return state;

        var newState = state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, null) };
        
        // Re-calculate validity for all junctions of this owner
        var cadences = ContentHost.GetContent<Cadences>();
        var ownerChar = new Character(owner);
        var remainingCadences = newState.AssignedCadences
            .Where(x => x.Value == owner)
            .Select(x => cadences.All.First(c => c.Name == x.Key))
            .ToList();

        var invalidJunctions = newState.Junctions
            .Where(j => j.Character.Name == owner)
            .Where(j => {
                string abilityName = GetJunctionAbilityName(j.Stat.Name);
                return !remainingCadences.Any(c => c.Abilities.Any(ua => ua.Ability.Name == abilityName && newState.UnlockedAbilities.Contains($"{c.Name}:{ua.Ability.Name}")));
            })
            .ToList();

        if (invalidJunctions.Any())
        {
            var junctions = newState.Junctions.ToBuilder();
            foreach (var ij in invalidJunctions) junctions.Remove(ij);
            newState = newState with { Junctions = junctions.ToImmutable() };
        }

        return newState;
    }

    private static GameState SpendResource(GameState state, SpendResourceAction a)
    {
        int current = state.Inventory.GetValueOrDefault(a.ItemName);
        int next = current - a.Quantity;
        if (next <= 0) return state with { Inventory = state.Inventory.Remove(a.ItemName) };
        return state with { Inventory = state.Inventory.SetItem(a.ItemName, next) };
    }

    private static string GetJunctionAbilityName(string statName) => statName switch
    {
        "Strength" => "J-Str",
        "Magic" => "J-Magic",
        "Vitality" => "J-Vit",
        "Speed" => "J-Speed",
        _ => "J-" + statName
    };
}

public static class NamedExtensions
{
    public static Dictionary<string, T> ToNamedDictionary<T>(this IEnumerable<T> source) where T : INamed
        => source.ToDictionary(x => x.Name);

    public static Dictionary<string, TValue> ToNamedDictionary<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valueSelector) where T : INamed
        => source.ToDictionary(x => x.Name, valueSelector);
}
