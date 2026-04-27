using System.Collections.Immutable;

namespace Mythril.Data;

public partial class GameStore
{
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
            AssignCadenceAction a => AssignCadence(state, a),
            UnassignCadenceAction a => UnassignCadence(state, a),
            JunctionMagicAction a => state with
            {
                Junctions = state.Junctions.RemoveAll(j => j.Character.Name == a.Character.Name && j.Stat.Name == a.Stat.Name).Add(new Junction(a.Character, a.Stat, a.Magic))
            },
            UnjunctionAction a => state with
            {
                Junctions = state.Junctions.RemoveAll(j => j.Character.Name == a.Character.Name && j.Stat.Name == a.Stat.Name)
            },
            TickAction a => state with
            {
                CurrentTime = state.CurrentTime + a.DeltaSeconds,
                ActiveQuests = [.. state.ActiveQuests.Select(q => q.IsCompleted ? q : q with { SecondsElapsed = q.SecondsElapsed + a.DeltaSeconds })]
            },
            SkipTimeAction a => state with { CurrentTime = state.CurrentTime + a.Seconds },
            UnlockAbilityAction a => UnlockAbility(state, a),
            UnlockCadenceAction a => state with { UnlockedCadenceNames = state.UnlockedCadenceNames.Add(a.CadenceName), HasUnseenCadence = true },
            ToggleAutoQuestAction a => state with { AutoQuestEnabled = state.AutoQuestEnabled.SetItem(a.CharacterName, a.Enabled) },
            TogglePinAction a => state with { PinnedItems = state.PinnedItems.Contains(a.ItemName) ? state.PinnedItems.Remove(a.ItemName) : state.PinnedItems.Add(a.ItemName) },
            ToggleRecipeStarAction a => state with { StarredRecipes = state.StarredRecipes.Contains(a.RecipeKey) ? state.StarredRecipes.Remove(a.RecipeKey) : state.StarredRecipes.Add(a.RecipeKey) },
            SetMagicCapacityAction a => state with { MagicCapacity = a.Capacity },
            ClearInventoryAction a => state with { Inventory = [] },
            SetStateAction a => a.NewState,
            AddStatBoostAction a => AddStatBoost(state, a.CharacterName, a.StatName, a.Amount),
            UnlockLocationAction a => state with { UnlockedLocationNames = state.UnlockedLocationNames.Add(a.LocationName) },
            SetHighlightedPathAction a => state with { HighlightedPath = a.Path },
            ClearHighlightedPathAction a => state with { HighlightedPath = [] },
            FinishQuestAction a => FinishQuest(state, a, out overflowItem, out overflowQty),
            SetActiveTabAction a => state with { ActiveTab = a.TabName },
            SetUnseenFlagsAction a => state with { HasUnseenCadence = a.Cadence, HasUnseenWorkshop = a.Workshop },
            SetTestModeAction a => state with { IsTestMode = a.IsTestMode },
            MarkContentSeenAction a => state with { SeenContent = state.SeenContent.Add(a.ContentId) },
            _ => state
        };
    }

    private static GameState StartQuest(GameState state, StartQuestAction a) =>
        // Costs are already paid in manager for now to keep it simple, but we could move it here
        state with { ActiveQuests = state.ActiveQuests.Add(a.Progress) };

    private static GameState UnlockAbility(GameState state, UnlockAbilityAction a)
    {
        var newState = state with { UnlockedAbilities = state.UnlockedAbilities.Add(a.AbilityKey) };

        // Update Magic Capacity
        var cadences = ContentHost.GetContent<Cadences>();
        var capacity = 30;
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
        var boosts = state.CharacterPermanentStatBoosts.GetValueOrDefault(characterName, []);
        var current = boosts.GetValueOrDefault(statName, 0);
        return state with
        {
            CharacterPermanentStatBoosts = state.CharacterPermanentStatBoosts.SetItem(characterName, boosts.SetItem(statName, current + amount))
        };
    }

    private static GameState FinishQuest(GameState state, FinishQuestAction a, out string? overflowItem, out int overflowQty)
    {
        overflowItem = null;
        overflowQty = 0;

        var progress = a.Progress;
        var item = progress.Item;
        var taskName = "Unknown";
        var characterName = progress.Character.Name;
        var details = "";

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

            if (questData.Type is QuestType.Single or QuestType.Unlock)
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
                        var isUsable = string.IsNullOrEmpty(loc.RequiredQuest) || nextState.CompletedQuests.Contains(loc.RequiredQuest);
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

            var abilityKey = $"{unlock.CadenceName}:{unlock.Ability.Name}";
            nextState = UnlockAbility(nextState, new UnlockAbilityAction(abilityKey));
        }
        else if (item is RefinementData refinement)
        {
            taskName = refinement.Name;
            details = refinement.Description;

            nextState = AddResource(nextState, new AddResourceAction(refinement.Recipe.OutputItem.Name, refinement.Recipe.OutputQuantity), out var oI, out var oQ);
            if (oI != null) { overflowItem = oI; overflowQty = oQ; }
        }

        // Remove from active quests
        return nextState with
        {
            ActiveQuests = nextState.ActiveQuests.RemoveAll(q => q.StartTime == progress.StartTime && q.Character.Name == progress.Character.Name)
        };
    }

    private static GameState AddResource(GameState state, AddResourceAction a, out string? overflowItem, out int overflowQty)
    {
        overflowItem = null;
        overflowQty = 0;

        var items = ContentHost.GetContent<Items>();
        var item = items.All.FirstOrDefault(i => i.Name == a.ItemName);

        var current = state.Inventory.GetValueOrDefault(a.ItemName);
        var next = current + a.Quantity;

        if (item.Name != null && item.ItemType == ItemType.Spell && next > state.MagicCapacity)
        {
            overflowQty = next - state.MagicCapacity;
            overflowItem = a.ItemName;
            next = state.MagicCapacity;
        }

        return state with { Inventory = state.Inventory.SetItem(a.ItemName, next) };
    }

    private static GameState AssignCadence(GameState state, AssignCadenceAction a)
    {
        // Get previous owner if any
        var prevOwner = state.AssignedCadences.GetValueOrDefault(a.CadenceName);

        var newState = state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, a.CharacterName) };

        // If it was "stolen", cleanup junctions for previous owner
        if (prevOwner != null && prevOwner != a.CharacterName)
        {
            newState = CleanupJunctions(newState, prevOwner);
        }

        return newState;
    }

    private static GameState CleanupJunctions(GameState state, string owner)
    {
        var cadences = ContentHost.GetContent<Cadences>();
        var remainingCadences = state.AssignedCadences
            .Where(x => x.Value == owner)
            .Select(x => cadences.All.FirstOrDefault(c => c.Name == x.Key))
            .Where(c => c.Name != null)
            .ToList();

        var invalidJunctions = state.Junctions
            .Where(j => j.Character.Name == owner)
            .Where(j =>
            {
                var abilityName = GetJunctionAbilityName(j.Stat.Name);
                return !remainingCadences.Any(c => c.Abilities.Any(ua => ua.Ability.Name == abilityName && state.UnlockedAbilities.Contains($"{c.Name}:{ua.Ability.Name}")));
            })
            .ToList();

        if (invalidJunctions.Count != 0)
        {
            var junctions = state.Junctions.ToBuilder();
            foreach (var ij in invalidJunctions) junctions.Remove(ij);
            return state with { Junctions = junctions.ToImmutable() };
        }

        return state;
    }

    private static GameState UnassignCadence(GameState state, UnassignCadenceAction a)
    {
        if (!state.AssignedCadences.TryGetValue(a.CadenceName, out var owner) || owner == null)
            return state;

        var newState = state with { AssignedCadences = state.AssignedCadences.SetItem(a.CadenceName, null) };
        return CleanupJunctions(newState, owner);
    }

    private static GameState SpendResource(GameState state, SpendResourceAction a)
    {
        var current = state.Inventory.GetValueOrDefault(a.ItemName);
        var next = current - a.Quantity;
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