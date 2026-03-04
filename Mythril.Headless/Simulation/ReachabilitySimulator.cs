using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class ReachabilitySimulator(
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
    public void Run()
    {
        var lattice = new LatticeSimulator(items, quests, questDetails, questUnlocks, questToCadenceUnlocks, cadences, locations, refinements, statAugments, stats);
        
        var seed = new SimulationSeed(
            ImmutableDictionary<string, int>.Empty,
            stats.All.ToImmutableDictionary(s => s.Name, _ => 10),
            ImmutableHashSet.Create<string>("Recruit"),
            ImmutableHashSet.Create<string>()
        );

        Console.WriteLine("Starting Lattice Simulation...");
        var finalState = lattice.Solve(seed);
        Console.WriteLine("Simulation Complete.");

        GenerateLatticeReport(finalState);
    }

    private void GenerateLatticeReport(GameState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Game Content Reachability Report (Lattice Model)");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // 1. Dead Content Detection
        sb.AppendLine("## 💀 Dead Content Detection");
        
        var unreachableQuests = quests.All.Where(q => state.QuestTime[q.Name] == double.PositiveInfinity).ToList();
        if (unreachableQuests.Any())
        {
            sb.AppendLine("### Unreachable Quests");
            foreach (var q in unreachableQuests) sb.AppendLine($"- {q.Name}");
        }
        else sb.AppendLine("✅ All quests reachable.");

        var unreachableResources = items.All.Where(i => state.ResourceTime[i.Name] == double.PositiveInfinity).ToList();
        if (unreachableResources.Any())
        {
            sb.AppendLine("\n### Unreachable Resources");
            foreach (var r in unreachableResources) sb.AppendLine($"- {r.Name}");
        }

        var unreachableCadences = cadences.All.Where(c => !state.UnlockedCadences.Contains(c.Name)).ToList();
        if (unreachableCadences.Any())
        {
            sb.AppendLine("\n### Unreachable Cadences");
            foreach (var c in unreachableCadences) sb.AppendLine($"- {c.Name}");
        }

        // 2. Stat Progression
        sb.AppendLine("\n## 📈 Maximum Achievable Stats");
        foreach (var stat in stats.All)
        {
            sb.AppendLine($"- **{stat.Name}**: {state.StatMax[stat.Name]}");
        }

        // 3. Time To New Content Metric
        sb.AppendLine("\n## ⏱️ Timeline Metrics");
        
        var events = new List<(string Name, double Time, string Type)>();
        foreach (var pair in state.QuestTime.Where(p => p.Value != double.PositiveInfinity)) events.Add((pair.Key, pair.Value, "Quest"));
        foreach (var pair in state.ResourceTime.Where(p => p.Value != double.PositiveInfinity && p.Value > 0)) events.Add((pair.Key, pair.Value, "Resource"));
        foreach (var a in state.UnlockedAbilities) events.Add((a, state.ResourceTime.GetValueOrDefault(a.Split(':')[1], 0), "Ability")); // Approximation

        var sortedEvents = events.OrderBy(e => e.Time).ToList();
        
        double longestStall = 0;
        double totalStall = 0;
        int stallCount = 0;
        double lastTime = 0;

        foreach (var e in sortedEvents)
        {
            double stall = e.Time - lastTime;
            if (stall > 0.001)
            {
                longestStall = Math.Max(longestStall, stall);
                totalStall += stall;
                stallCount++;
            }
            lastTime = e.Time;
        }

        sb.AppendLine($"- **Total Events**: {sortedEvents.Count}");
        sb.AppendLine($"- **Longest Progression Stall**: {longestStall:F1}s");
        sb.AppendLine($"- **Average Stall**: {(stallCount > 0 ? totalStall / stallCount : 0):F1}s");
        sb.AppendLine($"- **Estimated End-Game Time**: {lastTime:F1}s ({(lastTime / 60):F1}m)");

        Console.WriteLine(sb.ToString());
        System.IO.File.WriteAllText("simulation_report.md", sb.ToString());
        
        if (unreachableQuests.Any())
        {
            Console.WriteLine("[FAIL] reachability: Simulation failed: One or more quests are mathematically unreachable.");
            // Environment.Exit(1); // In a real CI env
        }
        else
        {
            Console.WriteLine("SIMULATION PASSED: All content reachable.");
        }
    }
}
