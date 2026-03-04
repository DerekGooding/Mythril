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

    public GameState Solve(SimulationSeed seed)
    {
        var state = Bottom(seed);
        int iterations = 0;

        while (true)
        {
            iterations++;
            var candidate = ApplyTransfers(state);
            var next = Join(state, candidate);

            if (next.Equals(state))
            {
                Console.WriteLine($"Lattice solver converged in {iterations} iterations.");
                return state;
            }

            // Monotonicity checks
            ValidateMonotonicity(state, next);

            state = next;
            
            if (iterations > 2000) throw new Exception("Solver failed to converge (monotonicity violation likely)");
        }
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
