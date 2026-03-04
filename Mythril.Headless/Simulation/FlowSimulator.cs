using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public record ActivityFlow(
    string Name,
    double Duration,
    ImmutableDictionary<string, int> Inputs,
    ImmutableDictionary<string, int> Outputs);

public record QuantitativeFlowState(
    ImmutableDictionary<string, double> ResourceNet,
    ImmutableHashSet<string> SustainableActivities,
    ImmutableHashSet<string> UnsustainableActivities);

public class FlowSimulator(
    Items items,
    Quests quests,
    QuestDetails questDetails,
    ItemRefinements refinements,
    Cadences cadences)
{
    public QuantitativeFlowState Solve(GameState reachabilityResult, SimulationSeed seed)
    {
        var flows = ExtractFlows(reachabilityResult);
        var flowMap = flows.ToDictionary(f => f.Name);
        
        var state = new QuantitativeFlowState(
            items.All.ToImmutableDictionary(i => i.Name, _ => 0.0),
            ImmutableHashSet<string>.Empty,
            ImmutableHashSet<string>.Empty
        );

        int iterations = 0;
        while (true)
        {
            iterations++;
            var next = Step(state, flows, flowMap, seed);
            if (next.SustainableActivities.Count == state.SustainableActivities.Count) break;
            state = next;
            
            if (iterations > 1000) break; // Safety break
        }

        // Identify reachable but unsustainable activities
        var allReachableRecurring = flows.Select(f => f.Name).ToHashSet();
        var unsustainable = allReachableRecurring.Except(state.SustainableActivities).ToImmutableHashSet();

        return state with { UnsustainableActivities = unsustainable };
    }

    private List<ActivityFlow> ExtractFlows(GameState reachabilityResult)
    {
        var flows = new List<ActivityFlow>();
        var questMap = quests.All.ToDictionary(q => q.Name);

        // 1. Recurring Quests
        foreach (var qTime in reachabilityResult.QuestTime)
        {
            if (qTime.Value == double.PositiveInfinity) continue;
            
            if (questMap.TryGetValue(qTime.Key, out var quest))
            {
                var detail = questDetails[quest];
                if (detail.Type != QuestType.Recurring) continue;

                flows.Add(new ActivityFlow(
                    quest.Name,
                    detail.DurationSeconds / (1.0 + (reachabilityResult.StatMax.GetValueOrDefault(detail.PrimaryStat, 10) / 100.0)),
                    detail.Requirements.ToImmutableDictionary(r => r.Item.Name, r => r.Quantity),
                    detail.Rewards.ToImmutableDictionary(r => r.Item.Name, r => r.Quantity)
                ));
            }
        }

        // 2. Refinements
        foreach (var refinementKvp in refinements.ByKey)
        {
            var ability = refinementKvp.Key;
            bool unlocked = reachabilityResult.UnlockedAbilities.Any(ua => ua.EndsWith($":{ability.Name}"));
            if (!unlocked) 
            {
                // Console.WriteLine($"DEBUG: Ability {ability.Name} not unlocked in reachability result.");
                continue;
            }

            foreach (var recipeKvp in refinementKvp.Value.Recipes)
            {
                var inputItem = recipeKvp.Key;
                var recipe = recipeKvp.Value;

                string flowName = $"{ability.Name}:{inputItem.Name}->{recipe.OutputItem.Name}";

                flows.Add(new ActivityFlow(
                    flowName,
                    15.0 / (1.0 + (reachabilityResult.StatMax.GetValueOrDefault(refinementKvp.Value.PrimaryStat, 10) / 100.0)),
                    new Dictionary<string, int> { { inputItem.Name, recipe.InputQuantity } }.ToImmutableDictionary(),
                    new Dictionary<string, int> { { recipe.OutputItem.Name, recipe.OutputQuantity } }.ToImmutableDictionary()
                ));
            }
        }

        return flows;
    }

    private QuantitativeFlowState Step(QuantitativeFlowState state, List<ActivityFlow> allFlows, Dictionary<string, ActivityFlow> flowMap, SimulationSeed seed)
    {
        var nextSustainable = state.SustainableActivities.ToBuilder();
        var nextNet = items.All.ToDictionary(i => i.Name, _ => 0.0);

        // Add infinite contribution from starting seed resources for sustainability check
        foreach (var starting in seed.StartingResources)
        {
            if (starting.Value > 0 && nextNet.ContainsKey(starting.Key))
                nextNet[starting.Key] = double.PositiveInfinity;
        }

        // Recompute net rates based on currently sustainable activities
        foreach (var flowName in state.SustainableActivities)
        {
            if (flowMap.TryGetValue(flowName, out var flow))
            {
                ApplyFlowToNet(flow, nextNet);
            }
        }

        // Try to activate new activities
        foreach (var flow in allFlows)
        {
            if (state.SustainableActivities.Contains(flow.Name)) continue;

            bool canSustain = true;
            foreach (var input in flow.Inputs)
            {
                // Must be produced at a higher rate than consumed
                if (nextNet.GetValueOrDefault(input.Key, 0) < (input.Value / flow.Duration))
                {
                    canSustain = false;
                    break;
                }
            }

            if (canSustain)
            {
                nextSustainable.Add(flow.Name);
                ApplyFlowToNet(flow, nextNet);
            }
        }

        return new QuantitativeFlowState(
            nextNet.ToImmutableDictionary(),
            nextSustainable.ToImmutable(),
            state.UnsustainableActivities
        );
    }

    private void ApplyFlowToNet(ActivityFlow flow, Dictionary<string, double> net)
    {
        foreach (var input in flow.Inputs)
        {
            net[input.Key] -= input.Value / flow.Duration;
        }
        foreach (var output in flow.Outputs)
        {
            net[output.Key] += output.Value / flow.Duration;
        }
    }

    public List<string> DetectInfiniteLoops(QuantitativeFlowState state, List<ActivityFlow> allFlows)
    {
        var loops = new List<string>();
        foreach (var resKvp in state.ResourceNet)
        {
            if (resKvp.Value > 0.0001)
            {
                if (IsPartOfCycle(resKvp.Key, state.SustainableActivities, allFlows))
                {
                    loops.Add(resKvp.Key);
                }
            }
        }
        return loops;
    }

    private bool IsPartOfCycle(string resourceName, ImmutableHashSet<string> activeActivities, List<ActivityFlow> allFlows)
    {
        var active = allFlows.Where(f => activeActivities.Contains(f.Name)).ToList();
        var visited = new HashSet<string>();
        return HasPathToResource(resourceName, resourceName, active, visited);
    }

    private bool HasPathToResource(string currentResource, string targetResource, List<ActivityFlow> flows, HashSet<string> visited)
    {
        if (visited.Contains(currentResource)) return false;
        visited.Add(currentResource);

        var producers = flows.Where(f => f.Inputs.ContainsKey(currentResource));
        foreach (var flow in producers)
        {
            foreach (var output in flow.Outputs)
            {
                if (output.Key == targetResource) return true;
                if (HasPathToResource(output.Key, targetResource, flows, visited)) return true;
            }
        }

        return false;
    }
}
