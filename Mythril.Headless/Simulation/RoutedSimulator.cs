using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class RoutedSimulator(
    Items items, Quests quests, QuestDetails questDetails, QuestUnlocks questUnlocks,
    QuestToCadenceUnlocks questToCadenceUnlocks, Cadences cadences, Locations locations,
    ItemRefinements refinements, StatAugments statAugments, Stats stats)
{
    private readonly HashSet<string> _farmingStack = [];
    private const string END_QUEST = "Defeat the Mythril Construct";

    public void Run()
    {
        Console.WriteLine("Starting Path-Routed Simulation...");
        var state = new SimulationState(stats);
        bool progressed = true; int steps = 0; const int MAX_STEPS = 10000;
        while (progressed && steps < MAX_STEPS)
        {
            steps++;
            progressed = AttemptStep(state, steps);
            if (state.CompletedQuests.Contains(END_QUEST)) { Console.WriteLine($"[SUCCESS] End Game reached!"); break; }
            if (state.CurrentTime > 3600 * 24 * 365) break; 
        }
        Console.WriteLine($"Routed Completion Time: {(state.CurrentTime / 60.0):F1} minutes");
        Console.WriteLine($"Total Quests Completed: {state.CompletedQuests.Count}");
        if (!state.CompletedQuests.Contains(END_QUEST)) {
            Console.WriteLine("[FAIL] End Game node never reached.");
            var endQuestObj = quests.All.First(q => q.Name == END_QUEST);
            var endQuestDet = questDetails[endQuestObj];
            Console.WriteLine($"[DEBUG] End Game Stats Required: {string.Join(", ", endQuestDet.RequiredStats?.Select(kvp => $"{kvp.Key}: {kvp.Value}") ?? ["None"])}");
            Console.WriteLine($"[DEBUG] Current Stats: {string.Join(", ", state.CurrentStats.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
            Console.WriteLine($"[DEBUG] Magic Capacity: {state.MagicCapacity}");
        }
    }
}
