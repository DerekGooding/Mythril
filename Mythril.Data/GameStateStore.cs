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
    ImmutableList<JournalEntry> Journal,
    ImmutableDictionary<string, ImmutableList<string>> CharacterMiniLogs,
    ImmutableHashSet<string> EverPerformedActivities,
    double CurrentTime,
    bool IsTestMode,
    bool HasUnseenCadence,
    bool HasUnseenWorkshop,
    string ActiveTab
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
        ActiveTab: "hand"
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
public record SkipTimeAction(double Seconds) : IGameAction;
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
public record AddToJournalAction(string TaskName, string CharacterName, string Details) : IGameAction;
public record ClearJournalAction() : IGameAction;
public record FinishQuestAction(QuestProgress Progress) : IGameAction;
public record SetActiveTabAction(string TabName) : IGameAction;
public record SetUnseenFlagsAction(bool Cadence, bool Workshop) : IGameAction;
public record SetTestModeAction(bool IsTestMode) : IGameAction;

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
    public event Action? OnJournalUpdated;

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
        if (action is AddToJournalAction || action is ClearJournalAction || action is SetStateAction || action is FinishQuestAction)
        {
            OnJournalUpdated?.Invoke();
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
            StartQuestAction a => StartQuest(state, a),
            CancelQuestAction a => state with { ActiveQuests = state.ActiveQuests.RemoveAll(q => q.StartTime == a.Progress.StartTime && q.Character.Name == a.Progress.Character.Name) },
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
            SkipTimeAction a => state with { CurrentTime = state.CurrentTime + a.Seconds },
            UnlockAbilityAction a => UnlockAbility(state, a),
            UnlockCadenceAction a => state with { UnlockedCadenceNames = state.UnlockedCadenceNames.Add(a.CadenceName), HasUnseenCadence = true },
            ToggleAutoQuestAction a => state with { AutoQuestEnabled = state.AutoQuestEnabled.SetItem(a.CharacterName, a.Enabled) },
            TogglePinAction a => state with { PinnedItems = state.PinnedItems.Contains(a.ItemName) ? state.PinnedItems.Remove(a.ItemName) : state.PinnedItems.Add(a.ItemName) },
            ToggleRecipeStarAction a => state with { StarredRecipes = state.StarredRecipes.Contains(a.RecipeKey) ? state.StarredRecipes.Remove(a.RecipeKey) : state.StarredRecipes.Add(a.RecipeKey) },
            SetMagicCapacityAction a => state with { MagicCapacity = a.Capacity },
            ClearInventoryAction a => state with { Inventory = ImmutableDictionary<string, int>.Empty },
            SetStateAction a => a.NewState,
            AddStatBoostAction a => AddStatBoost(state, a.CharacterName, a.StatName, a.Amount),
            UnlockLocationAction a => state with { UnlockedLocationNames = state.UnlockedLocationNames.Add(a.LocationName) },
            SetHighlightedPathAction a => state with { HighlightedPath = a.Path },
            ClearHighlightedPathAction a => state with { HighlightedPath = ImmutableHashSet<string>.Empty },
            AddToJournalAction a => AddToJournal(state, a),
            ClearJournalAction a => state with { 
                Journal = ImmutableList<JournalEntry>.Empty, 
                CharacterMiniLogs = ImmutableDictionary<string, ImmutableList<string>>.Empty,
                EverPerformedActivities = ImmutableHashSet<string>.Empty
            },
            FinishQuestAction a => FinishQuest(state, a, out overflowItem, out overflowQty),
            SetActiveTabAction a => state with { ActiveTab = a.TabName },
            SetUnseenFlagsAction a => state with { HasUnseenCadence = a.Cadence, HasUnseenWorkshop = a.Workshop },
            SetTestModeAction a => state with { IsTestMode = a.IsTestMode },
            _ => state
        };
    }

    private static GameState StartQuest(GameState state, StartQuestAction a)
    {
        // Costs are already paid in manager for now to keep it simple, but we could move it here
        return state with { ActiveQuests = state.ActiveQuests.Add(a.Progress) };
    }

    private static GameState UnlockAbility(GameState state, UnlockAbilityAction a)
    {
        var newState = state with { UnlockedAbilities = state.UnlockedAbilities.Add(a.AbilityKey) };
        
        // Update Magic Capacity
        var cadences = ContentHost.GetContent<Cadences>();
        int capacity = 30;
        foreach (var abilityKey in newState.UnlockedAbilities)
        {
            var parts = abilityKey.Split(':');
            if (parts.Length < 2) continue;
            var cadenceName = parts[0];
            var abilityName = parts[1];
            
            var cadence = cadences.All.FirstOrDefault(c => c.Name == cadenceName);
            if (cadence.Name == null) continue;
            
            var unlock = cadence.Abilities.FirstOrDefault(au => au.Ability.Name == abilityName);
            if (unlock.Ability.Name != null && unlock.Ability.Effects != null)
            {
                foreach (var effect in unlock.Ability.Effects)
                {
                    if (effect.Type == EffectType.MagicCapacity)
                    {
                        capacity = Math.Max(capacity, effect.Value);
                    }
                }
            }
        }

        return newState with { MagicCapacity = capacity };
    }

    private static GameState AddStatBoost(GameState state, string characterName, string statName, int amount)
    {
        var boosts = state.CharacterPermanentStatBoosts.GetValueOrDefault(characterName, ImmutableDictionary<string, int>.Empty);
        int current = boosts.GetValueOrDefault(statName, 0);
        return state with {
            CharacterPermanentStatBoosts = state.CharacterPermanentStatBoosts.SetItem(characterName, boosts.SetItem(statName, current + amount))
        };
    }

    private static GameState FinishQuest(GameState state, FinishQuestAction a, out string? overflowItem, out int overflowQty)
    {
        overflowItem = null;
        overflowQty = 0;

        var progress = a.Progress;
        object item = progress.Item;
        string taskName = "Unknown";
        string characterName = progress.Character.Name;
        string details = "";

        var nextState = state;

        if (item is QuestData questData)
        {
            taskName = questData.Name;
            details = "Completed " + questData.Name;
            
            // Add rewards
            if (questData.Rewards != null)
            {
                foreach (var reward in questData.Rewards)
                {
                    nextState = AddResource(nextState, new AddResourceAction(reward.Item.Name, reward.Quantity), out var oI, out var oQ);
                    if (oI != null) { overflowItem = oI; overflowQty = oQ; }
                }
            }

            // Stat rewards
            if (questData.StatRewards != null)
            {
                foreach (var statRew in questData.StatRewards)
                {
                    nextState = AddStatBoost(nextState, characterName, statRew.Key, statRew.Value);
                }
            }

            // Unlocks
            nextState = nextState with { CompletedQuests = nextState.CompletedQuests.Add(questData.Quest.Name) };

            if (questData.Type == QuestType.Single || questData.Type == QuestType.Unlock)
            {
                // Unlock Cadences
                var qToC = ContentHost.GetContent<QuestToCadenceUnlocks>();
                var unlockedCadences = qToC[questData.Quest];
                foreach (var cadence in unlockedCadences)
                {
                    nextState = nextState with { UnlockedCadenceNames = nextState.UnlockedCadenceNames.Add(cadence.Name) };
                }

                // Discover locations
                var locations = ContentHost.GetContent<Locations>();
                
                foreach (var loc in locations.All)
                {
                    if (!nextState.UnlockedLocationNames.Contains(loc.Name))
                    {
                        bool isUsable = string.IsNullOrEmpty(loc.RequiredQuest) || nextState.CompletedQuests.Contains(loc.RequiredQuest);
                        if (isUsable)
                        {
                            nextState = nextState with { UnlockedLocationNames = nextState.UnlockedLocationNames.Add(loc.Name) };
                        }
                    }
                }
            }
        }
        else if (item is CadenceUnlock unlock)
        {
            taskName = unlock.Ability.Name;
            details = $"Researched {unlock.Ability.Name} for {unlock.CadenceName}";
            
            string abilityKey = $"{unlock.CadenceName}:{unlock.Ability.Name}";
            nextState = UnlockAbility(nextState, new UnlockAbilityAction(abilityKey));
        }
        else if (item is RefinementData refinement)
        {
            taskName = refinement.Name;
            details = refinement.Description;
            
            nextState = AddResource(nextState, new AddResourceAction(refinement.Recipe.OutputItem.Name, refinement.Recipe.OutputQuantity), out var oI, out var oQ);
            if (oI != null) { overflowItem = oI; overflowQty = oQ; }
        }

        // Journal Entry
        nextState = AddToJournal(nextState, new AddToJournalAction(taskName, characterName, details));

        // Remove from active quests
        nextState = nextState with { 
            ActiveQuests = nextState.ActiveQuests.RemoveAll(q => q.StartTime == progress.StartTime && q.Character.Name == progress.Character.Name) 
        };

        return nextState;
    }

    private static GameState AddToJournal(GameState state, AddToJournalAction a)
    {
        bool isFirstTime = !state.EverPerformedActivities.Contains(a.TaskName);
        var entry = new JournalEntry(a.TaskName, a.CharacterName, a.Details, DateTime.Now, isFirstTime);
        
        var newJournal = state.Journal.Insert(0, entry);
        if (newJournal.Count > 50) newJournal = newJournal.RemoveAt(newJournal.Count - 1);

        var characterLog = state.CharacterMiniLogs.GetValueOrDefault(a.CharacterName, ImmutableList<string>.Empty);
        characterLog = characterLog.Add(a.TaskName);
        if (characterLog.Count > 3) characterLog = characterLog.RemoveAt(0);

        return state with { 
            Journal = newJournal,
            CharacterMiniLogs = state.CharacterMiniLogs.SetItem(a.CharacterName, characterLog),
            EverPerformedActivities = state.EverPerformedActivities.Add(a.TaskName)
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
            .Select(x => cadences.All.FirstOrDefault(c => c.Name == x.Key))
            .Where(c => c.Name != null)
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
