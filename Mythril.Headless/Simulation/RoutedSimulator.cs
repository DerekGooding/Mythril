using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public class RoutedSimulator(
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
        bool progressed = true; int steps = 0; const int MAX_STEPS = 5000;
        while (progressed && steps < MAX_STEPS)
        {
            steps++;
            progressed = AttemptStep(state);
            if (state.CompletedQuests.Contains(END_QUEST)) { Console.WriteLine($"[SUCCESS] End Game reached!"); break; }
            if (state.CurrentTime > 3600 * 24 * 365) break; 
        }
        Console.WriteLine($"Routed Completion Time: {(state.CurrentTime / 60.0):F1} minutes");
        Console.WriteLine($"Total Quests Completed: {state.CompletedQuests.Count}");
        if (!state.CompletedQuests.Contains(END_QUEST)) Console.WriteLine("[FAIL] End Game node never reached.");
    }

    private bool AttemptStep(SimulationState state)
    {
        var available = GetAvailableQuests(state);
        var targetable = available.Where(q => !state.CompletedQuests.Contains(q.Quest.Name)).OrderBy(q => q.Detail.DurationSeconds).ToList();
        
        foreach (var target in targetable) {
            if (CanAffordEventually(state, target.Detail.Requirements)) {
                ExecuteQuest(state, target.Quest, target.Detail); return true;
            }
        }

        var uncompletedSingles = locations.All.SelectMany(l => l.Quests ?? []).Where(q => (questDetails[q].Type != QuestType.Recurring) && !state.CompletedQuests.Contains(q.Name)).ToList();
        foreach (var q in uncompletedSingles) {
            var prereqs = questUnlocks[q]; if (prereqs == null) continue;
            foreach (var pre in prereqs) {
                if (!state.CompletedQuests.Contains(pre.Name)) {
                    var match = available.FirstOrDefault(x => x.Quest.Name == pre.Name);
                    if (match.Quest != null && match.Detail != null && CanAffordEventually(state, match.Detail.Requirements)) {
                        ExecuteQuest(state, match.Quest, match.Detail); return true;
                    }
                }
            }
        }

        foreach (var cadName in state.UnlockedCadences) {
            var cadence = cadences.All.FirstOrDefault(c => c.Name == cadName);
            if (cadence.Name == null || cadence.Abilities == null) continue;
            foreach (var unlock in cadence.Abilities) {
                string key = $"{cadName}:{unlock.Ability.Name}";
                if (state.UnlockedAbilities.Contains(key)) continue;
                if (unlock.Requirements != null && CanAffordEventually(state, unlock.Requirements)) {
                    foreach (var req in unlock.Requirements) FarmResource(state, req.Item, req.Quantity);
                    foreach (var req in unlock.Requirements) SubtractFromInventory(state, req.Item.Name, req.Quantity);
                    state.UnlockedAbilities.Add(key);
                    if (unlock.Ability.Metadata?.TryGetValue("MagicCapacity", out var capStr) == true && int.TryParse(capStr, out var capVal))
                        state.MagicCapacity = Math.Max(state.MagicCapacity, capVal);
                    UpdateStats(state); return true;
                }
            }
        }
        return false;
    }

    private void SubtractFromInventory(SimulationState state, string itemName, long quantity)
    {
        if (state.Inventory.TryGetValue(itemName, out long current))
            state.Inventory[itemName] = current - quantity;
        else
            state.Inventory[itemName] = -quantity;
    }

    private long GetFromInventory(SimulationState state, string itemName)
    {
        return state.Inventory.TryGetValue(itemName, out long current) ? current : 0;
    }

    private bool CanAffordEventually(SimulationState state, ItemQuantity[] requirements)
    {
        foreach (var req in requirements) {
            if (req.Item == null) continue;
            if (GetFromInventory(state, req.Item.Name) >= req.Quantity) continue;
            var source = GetBestSource(state, req.Item);
            if (source.Quest == null && source.Ability == null) return false;
        }
        return true;
    }

    private List<(Quest Quest, QuestDetail Detail)> GetAvailableQuests(SimulationState state)
    {
        var available = new List<(Quest, QuestDetail)>();
        foreach (var loc in locations.All) {
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) continue;
            if (loc.Quests == null) continue;
            foreach (var q in loc.Quests) {
                if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) continue;
                var det = questDetails[q];
                if (det.RequiredStats?.All(rs => state.CurrentStats.GetValueOrDefault(rs.Key, 0) >= rs.Value) ?? true) available.Add((q, det));
            }
        }
        return available;
    }

    private void ExecuteQuest(SimulationState state, Quest q, QuestDetail detail)
    {
        if (detail.Requirements != null) {
            foreach (var req in detail.Requirements) FarmResource(state, req.Item, req.Quantity);
            foreach (var req in detail.Requirements) SubtractFromInventory(state, req.Item.Name, req.Quantity);
        }
        state.CurrentTime += detail.DurationSeconds * Math.Pow(0.75, (state.CurrentStats.GetValueOrDefault(detail.PrimaryStat ?? "Vitality", 10) - 10) / 10.0);
        state.CompletedQuests.Add(q.Name);
        if (detail.Rewards != null) foreach (var rew in detail.Rewards) state.Inventory[rew.Item.Name] = GetFromInventory(state, rew.Item.Name) + rew.Quantity;
        var cads = questToCadenceUnlocks[q];
        if (cads != null) foreach (var cad in cads) state.UnlockedCadences.Add(cad.Name);
        UpdateStats(state);
    }

    private bool FarmResource(SimulationState state, Item item, long quantityNeeded)
    {
        if (_farmingStack.Contains(item.Name)) return false;
        _farmingStack.Add(item.Name);
        try {
            long current = GetFromInventory(state, item.Name);
            if (current >= quantityNeeded) return true;
            var source = GetBestSource(state, item);
            var sq = source.Quest; var sd = source.Detail;
            if (sq != null && sd != null) {
                var rewards = ((QuestDetail)sd).Rewards;
                if (rewards != null) {
                    var reward = rewards.First(r => r.Item == item);
                    int runs = (int)Math.Min(1000, Math.Ceiling((double)(quantityNeeded - current) / reward.Quantity));
                    for (int i = 0; i < runs; i++) ExecuteQuest(state, (Quest)sq, (QuestDetail)sd);
                    return GetFromInventory(state, item.Name) >= quantityNeeded;
                }
            } else if (source.Ability != null && source.Recipes != null) {
                var recipe = source.Recipes.First(x => x.Value.OutputItem == item);
                int runs = (int)Math.Min(1000, Math.Ceiling((double)(quantityNeeded - current) / recipe.Value.OutputQuantity));
                for (int i = 0; i < runs; i++) ExecuteRefinement(state, (CadenceAbility)source.Ability, (string)source.PrimaryStat!, recipe.Key, recipe.Value);
                return GetFromInventory(state, item.Name) >= quantityNeeded;
            }
            return false;
        } finally { _farmingStack.Remove(item.Name); }
    }

    private void ExecuteRefinement(SimulationState state, CadenceAbility ability, string primaryStat, Item input, Recipe recipe)
    {
        FarmResource(state, input, recipe.InputQuantity);
        SubtractFromInventory(state, input.Name, recipe.InputQuantity);
        state.CurrentTime += 15.0 * Math.Pow(0.75, (state.CurrentStats.GetValueOrDefault(primaryStat, 10) - 10) / 10.0);
        state.Inventory[recipe.OutputItem.Name] = GetFromInventory(state, recipe.OutputItem.Name) + recipe.OutputQuantity;
        UpdateStats(state);
    }

    private ActivitySource GetBestSource(SimulationState state, Item item)
    {
        var recurring = locations.All.SelectMany(l => l.Quests ?? Array.Empty<Quest>()).Where(q => {
            var loc = locations.All.First(x => x.Quests != null && x.Quests.Contains(q));
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) return false;
            if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) return false;
            var d = questDetails[q]; return d.Type == QuestType.Recurring && (d.Rewards?.Any(r => r.Item == item) ?? false);
        }).OrderByDescending(q => (double)(questDetails[q].Rewards?.First(r => r.Item == item).Quantity ?? 0) / questDetails[q].DurationSeconds).FirstOrDefault();
        if (recurring != null && recurring.Name != null) return new ActivitySource { Quest = recurring, Detail = questDetails[recurring] };
        var refMatch = refinements.ByKey.Where(r => state.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")) && (r.Value.Recipes?.Values.Any(rec => rec.OutputItem == item) ?? false)).FirstOrDefault();
        if (refMatch.Key != null && refMatch.Key.Name != null) return new ActivitySource { Ability = refMatch.Key, PrimaryStat = refMatch.Value.PrimaryStat, Recipes = refMatch.Value.Recipes };
        return new ActivitySource();
    }

    private void UpdateStats(SimulationState state)
    {
        if (state.CurrentStats.GetValueOrDefault("Strength", 0) >= 60) state.UnlockedCadences.Add("Geologist");
        if (state.CurrentStats.GetValueOrDefault("Speed", 0) >= 60) state.UnlockedCadences.Add("Tide-Caller");
        if (state.CurrentStats.GetValueOrDefault("Vitality", 0) >= 60) state.UnlockedCadences.Add("The Sentinel");
        if (state.CurrentStats.GetValueOrDefault("Magic", 0) >= 100) state.UnlockedCadences.Add("Scholar");
        if (state.CurrentStats.GetValueOrDefault("Strength", 0) >= 100 && state.CurrentStats.GetValueOrDefault("Speed", 0) >= 100) state.UnlockedCadences.Add("Slayer");

        foreach (var stat in stats.All) {
            string jAbil = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
            if (state.UnlockedAbilities.Any(ua => ua.EndsWith($":{jAbil}"))) {
                var best = items.All.Where(i => i.ItemType == ItemType.Spell && GetFromInventory(state, i.Name) > 0)
                    .Select(i => (Item: i, Augment: statAugments[i].FirstOrDefault(a => a.Stat.Name == stat.Name)))
                    .Where(x => x.Augment != null && x.Augment.Stat != null && x.Augment.Stat.Name != null).OrderByDescending(x => x.Augment.ModifierAtFull).FirstOrDefault();
                if (best.Item != null) {
                    int val = 10 + (int)(state.MagicCapacity * (best.Augment.ModifierAtFull / 100.0));
                    state.CurrentStats[stat.Name] = Math.Max(state.CurrentStats.GetValueOrDefault(stat.Name, 25), Math.Min(255, val));
                }
            }
        }
    }
}
