using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
public record SetStateAction(GameState NewState) : IGameAction;
public record AddStatBoostAction(string CharacterName, string StatName, int Amount) : IGameAction;
public record UnlockLocationAction(string LocationName) : IGameAction;
public record SetHighlightedPathAction(ImmutableHashSet<string> Path) : IGameAction;
public record ClearHighlightedPathAction() : IGameAction;

public class GameStore
{
    public GameState State { get; private set; } = GameState.Initial;
    public event Action<GameState>? OnStateChanged;

    public void Dispatch(IGameAction action)
    {
        State = Reduce(State, action);
        OnStateChanged?.Invoke(State);
    }

    private static GameState Reduce(GameState state, IGameAction action) => action switch
    {
        AddResourceAction a => state with { Inventory = state.Inventory.SetItem(a.ItemName, state.Inventory.GetValueOrDefault(a.ItemName) + a.Quantity) },
        SpendResourceAction a => state with { Inventory = state.Inventory.SetItem(a.ItemName, Math.Max(0, state.Inventory.GetValueOrDefault(a.ItemName) - a.Quantity)) },
        CompleteQuestAction a => state with { CompletedQuests = state.CompletedQuests.Add(a.Quest.Name) },
        LockQuestAction a => state with { CompletedQuests = state.CompletedQuests.Remove(a.Quest.Name) },
        StartQuestAction a => state with { ActiveQuests = state.ActiveQuests.Add(a.Progress) },
        CancelQuestAction a => state with { ActiveQuests = state.ActiveQuests.Remove(a.Progress) },
        AssignCadenceAction a => state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, a.CharacterName) },
        UnassignCadenceAction a => state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, null) },
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
