using System;
using System.Collections.Generic;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public class SimulationState(Stats stats)
{
    public double CurrentTime { get; set; } = 0;
    public Dictionary<string, long> Inventory { get; } = [];
    public HashSet<string> CompletedQuests { get; } = [];
    public HashSet<string> UnlockedCadences { get; } = ["Recruit"];
    public HashSet<string> UnlockedAbilities { get; } = [];
    public Dictionary<string, int> CurrentStats { get; } = InitializeStats(stats);
    public int MagicCapacity { get; set; } = 30;

    private static Dictionary<string, int> InitializeStats(Stats stats)
    {
        var dict = new Dictionary<string, int>();
        foreach (var s in stats.All) dict[s.Name] = 25;
        return dict;
    }
}

public class ActivitySource
{
    public Quest? Quest { get; set; }
    public QuestDetail? Detail { get; set; }
    public CadenceAbility? Ability { get; set; }
    public string? PrimaryStat { get; set; }
    public Dictionary<Item, Recipe>? Recipes { get; set; }
}
