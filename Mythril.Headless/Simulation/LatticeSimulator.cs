using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

/// <summary>
/// Represents the immutable state of the simulation at a point in the lattice.
/// </summary>
public record GameState(
    ImmutableDictionary<string, double> ResourceTime,
    ImmutableDictionary<string, double> QuestTime,
    ImmutableDictionary<string, int> StatMax,
    ImmutableHashSet<string> UnlockedAbilities,
    ImmutableHashSet<string> UnlockedCadences,
    int MagicCapacity)
{
    /// <summary>
    /// Partial ordering: A <= B if B is "better" or "more advanced" than A.
    /// - Times in B must be <= Times in A (reached earlier)
    /// - Stats in B must be >= Stats in A
    /// - Unlocks in B must be supersets of A
    /// </summary>
    public bool LessThanOrEqual(GameState other)
    {
        foreach (var kvp in ResourceTime)
            if (other.ResourceTime.GetValueOrDefault(kvp.Key, double.PositiveInfinity) > kvp.Value) return false;

        foreach (var kvp in QuestTime)
            if (other.QuestTime.GetValueOrDefault(kvp.Key, double.PositiveInfinity) > kvp.Value) return false;

        foreach (var kvp in StatMax)
            if (other.StatMax.GetValueOrDefault(kvp.Key, 0) < kvp.Value) return false;

        if (!UnlockedAbilities.IsSubsetOf(other.UnlockedAbilities)) return false;
        if (!UnlockedCadences.IsSubsetOf(other.UnlockedCadences)) return false;
        if (MagicCapacity > other.MagicCapacity) return false;

        return true;
    }
}

public record SimulationSeed(
    ImmutableDictionary<string, int> StartingResources,
    ImmutableDictionary<string, int> StartingStats,
    ImmutableHashSet<string> StartingUnlockedCadences,
    ImmutableHashSet<string> StartingUnlockedAbilities);

public partial class LatticeSimulator(
    Items items,
    Quests quests,
    QuestDetails questDetails,
    QuestUnlocks questUnlocks,
    QuestToCadenceUnlocks questToCadenceUnlocks,
    Cadences cadences,
    Locations locations,
    ItemRefinements refinements,
    StatAugments statAugments,
    Stats stats)
{
    public GameState Bottom(SimulationSeed seed)
    {
        var resourceTimes = items.All.ToImmutableDictionary(i => i.Name, _ => double.PositiveInfinity);
        // Set initial resources to time 0
        foreach (var res in seed.StartingResources)
        {
            if (resourceTimes.ContainsKey(res.Key))
                resourceTimes = resourceTimes.SetItem(res.Key, 0.0);
        }

        return new GameState(
            resourceTimes,
            quests.All.ToImmutableDictionary(q => q.Name, _ => double.PositiveInfinity),
            seed.StartingStats,
            seed.StartingUnlockedAbilities,
            seed.StartingUnlockedCadences,
            30 // Initial magic capacity
        );
    }

    public GameState Join(GameState a, GameState b)
    {
        var resourceTime = a.ResourceTime.ToBuilder();
        foreach (var kvp in b.ResourceTime)
            resourceTime[kvp.Key] = Math.Min(a.ResourceTime.GetValueOrDefault(kvp.Key, double.PositiveInfinity), kvp.Value);

        var questTime = a.QuestTime.ToBuilder();
        foreach (var kvp in b.QuestTime)
            questTime[kvp.Key] = Math.Min(a.QuestTime.GetValueOrDefault(kvp.Key, double.PositiveInfinity), kvp.Value);

        var statMax = a.StatMax.ToBuilder();
        foreach (var kvp in b.StatMax)
            statMax[kvp.Key] = Math.Max(a.StatMax.GetValueOrDefault(kvp.Key, 0), kvp.Value);

        return new GameState(
            resourceTime.ToImmutable(),
            questTime.ToImmutable(),
            statMax.ToImmutable(),
            a.UnlockedAbilities.Union(b.UnlockedAbilities),
            a.UnlockedCadences.Union(b.UnlockedCadences),
            Math.Max(a.MagicCapacity, b.MagicCapacity)
        );
    }

    private enum NodeType { Quest, Resource, Refinement, Ability, Stat, Cadence, MagicCapacity }
    private record WorklistItem(NodeType Type, string Name);

    private Dictionary<WorklistItem, List<WorklistItem>> _dependents = new();

    private void BuildDependencyGraph()
    {
        _dependents.Clear();
        void AddDep(WorklistItem trigger, WorklistItem dependent)
        {
            if (!_dependents.ContainsKey(trigger)) _dependents[trigger] = new();
            if (!_dependents[trigger].Contains(dependent)) _dependents[trigger].Add(dependent);
        }

        var itemMap = items.All.ToDictionary(i => i.Name);

        // 1. Quest Dependencies
        foreach (var quest in quests.All)
        {
            var detail = questDetails[quest];
            var qItem = new WorklistItem(NodeType.Quest, quest.Name);

            // Item -> Quest
            foreach (var req in detail.Requirements)
            {
                AddDep(new WorklistItem(NodeType.Resource, req.Item.Name), qItem);
            }

            // Stat -> Quest (Requirements and Scaling)
            AddDep(new WorklistItem(NodeType.Stat, detail.PrimaryStat), qItem);
            if (detail.RequiredStats != null)
            {
                foreach (var reqS in detail.RequiredStats.Keys)
                {
                    AddDep(new WorklistItem(NodeType.Stat, reqS), qItem);
                }
            }

            // Quest -> Quest (Prerequisites)
            foreach (var reqQ in questUnlocks[quest])
            {
                AddDep(new WorklistItem(NodeType.Quest, reqQ.Name), qItem);
            }

            // Quest -> Item (Rewards)
            foreach (var reward in detail.Rewards)
            {
                AddDep(qItem, new WorklistItem(NodeType.Resource, reward.Item.Name));
            }

            // Quest -> Cadence (Unlocks)
            foreach (var cad in questToCadenceUnlocks[quest])
            {
                AddDep(qItem, new WorklistItem(NodeType.Cadence, cad.Name));
            }

            // Location requirements (Quest -> Quest)
            foreach (var loc in locations.All)
            {
                if (loc.Quests.Contains(quest) && !string.IsNullOrEmpty(loc.RequiredQuest))
                {
                    AddDep(new WorklistItem(NodeType.Quest, loc.RequiredQuest), qItem);
                }
            }
        }

        // 2. Refinement Dependencies
        foreach (var refinementKvp in refinements.ByKey)
        {
            var ability = refinementKvp.Key;
            var aItem = new WorklistItem(NodeType.Ability, ability.Name);

            foreach (var recipeKvp in refinementKvp.Value.Recipes)
            {
                var inputItem = recipeKvp.Key;
                var recipe = recipeKvp.Value;
                var rItem = new WorklistItem(NodeType.Refinement, $"{ability.Name}:{inputItem.Name}");

                AddDep(aItem, rItem);
                AddDep(new WorklistItem(NodeType.Resource, inputItem.Name), rItem);
                AddDep(new WorklistItem(NodeType.Stat, refinementKvp.Value.PrimaryStat), rItem);
                AddDep(rItem, new WorklistItem(NodeType.Resource, recipe.OutputItem.Name));
            }
        }

        // 3. Ability Dependencies
        foreach (var cad in cadences.All)
        {
            var cItem = new WorklistItem(NodeType.Cadence, cad.Name);
            foreach (var unlock in cad.Abilities)
            {
                var aItem = new WorklistItem(NodeType.Ability, unlock.Ability.Name);
                AddDep(cItem, aItem);
                foreach (var req in unlock.Requirements)
                {
                    AddDep(new WorklistItem(NodeType.Resource, req.Item.Name), aItem);
                }

                // Ability -> Capacity
                if (unlock.Ability.Metadata != null && unlock.Ability.Metadata.ContainsKey("MagicCapacity"))
                {
                    AddDep(aItem, new WorklistItem(NodeType.MagicCapacity, ""));
                }
            }
        }

        // 4. Stat Dependencies
        foreach (var stat in stats.All)
        {
            var sItem = new WorklistItem(NodeType.Stat, stat.Name);
            string abilityName = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
            
            AddDep(new WorklistItem(NodeType.Ability, abilityName), sItem);
            AddDep(new WorklistItem(NodeType.MagicCapacity, ""), sItem);

            foreach (var item in items.All.Where(i => i.ItemType == ItemType.Spell))
            {
                AddDep(new WorklistItem(NodeType.Resource, item.Name), sItem);
            }

            // Hidden Cadence unlocks
            if (stat.Name == "Strength" || stat.Name == "Speed" || stat.Name == "Vitality" || stat.Name == "Magic")
            {
                AddDep(sItem, new WorklistItem(NodeType.Cadence, "HIDDEN"));
            }
        }

        // 5. Item -> everything else (Delta propagation helper)
        // If an item time changes, all its dependents need update.
        // This is already covered by Resource -> Quest, Resource -> Refinement, etc.
    }

    public GameState Solve(SimulationSeed seed)
    {
        BuildDependencyGraph();
        var state = Bottom(seed);
        
        var worklist = new Queue<WorklistItem>();
        var inWorklist = new HashSet<WorklistItem>();

        void Enqueue(WorklistItem item)
        {
            if (inWorklist.Add(item)) worklist.Enqueue(item);
        }

        // Initial seeding
        foreach (var res in seed.StartingResources) Enqueue(new WorklistItem(NodeType.Resource, res.Key));
        foreach (var stat in seed.StartingStats) Enqueue(new WorklistItem(NodeType.Stat, stat.Key));
        foreach (var ability in seed.StartingUnlockedAbilities) Enqueue(new WorklistItem(NodeType.Ability, ability.Split(':').Last()));
        foreach (var cad in seed.StartingUnlockedCadences) Enqueue(new WorklistItem(NodeType.Cadence, cad));
        
        // Add all quests that have no prerequisites to start the chain
        foreach (var quest in quests.All)
        {
            if (questUnlocks[quest].Length == 0) Enqueue(new WorklistItem(NodeType.Quest, quest.Name));
        }

        // Capacity and StatPass
        Enqueue(new WorklistItem(NodeType.MagicCapacity, ""));
        foreach (var stat in stats.All) Enqueue(new WorklistItem(NodeType.Stat, stat.Name));
        Enqueue(new WorklistItem(NodeType.Cadence, "HIDDEN"));

        int iterations = 0;
        while (worklist.Count > 0)
        {
            iterations++;
            var item = worklist.Dequeue();
            inWorklist.Remove(item);

            var (changed, nextState) = UpdateNode(item, state);
            if (changed)
            {
                // Monotonicity check
                ValidateMonotonicity(state, nextState);
                state = nextState;

                if (_dependents.TryGetValue(item, out var deps))
                {
                    foreach (var dep in deps) Enqueue(dep);
                }
            }

            if (iterations > 100000) throw new Exception("Solver failed to converge (worklist explosion)");
        }

        Console.WriteLine($"Worklist solver converged in {iterations} steps.");
        return state;
    }

    private (bool, GameState) UpdateNode(WorklistItem item, GameState state)
    {
        return item.Type switch
        {
            NodeType.Quest => UpdateQuest(item.Name, state),
            NodeType.Refinement => UpdateRefinement(item.Name, state),
            NodeType.Ability => UpdateAbility(item.Name, state),
            NodeType.Stat => UpdateStat(item.Name, state),
            NodeType.Cadence => UpdateCadence(item.Name, state),
            NodeType.MagicCapacity => UpdateCapacity(state),
            _ => (false, state)
        };
    }

    private void ValidateMonotonicity(GameState oldState, GameState newState)
    {
        foreach (var kvp in oldState.ResourceTime)
            if (newState.ResourceTime[kvp.Key] > kvp.Value) throw new Exception($"Non-monotonic resource time: {kvp.Key}");

        foreach (var kvp in oldState.QuestTime)
            if (newState.QuestTime[kvp.Key] > kvp.Value) throw new Exception($"Non-monotonic quest time: {kvp.Key}");

        foreach (var kvp in oldState.StatMax)
            if (newState.StatMax[kvp.Key] < kvp.Value) throw new Exception($"Non-monotonic stat max: {kvp.Key}");

        if (!oldState.UnlockedAbilities.IsSubsetOf(newState.UnlockedAbilities)) throw new Exception("Unlocked abilities shrank");
        if (!oldState.UnlockedCadences.IsSubsetOf(newState.UnlockedCadences)) throw new Exception("Unlocked cadences shrank");
    }
}
