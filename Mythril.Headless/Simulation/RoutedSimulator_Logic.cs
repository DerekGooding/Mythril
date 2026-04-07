using System;
using System.Collections.Generic;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class RoutedSimulator
{
    private bool AttemptStep(SimulationState state, int steps)
    {
        var available = GetAvailableQuests(state);

        // Priority 0: End Game if available and affordable
        var endQuest = available.FirstOrDefault(q => q.Quest.Name == END_QUEST);
        if (endQuest.Quest.Name != null && CanAffordEventually(state, endQuest.Detail.Requirements)) {
            ExecuteQuest(state, endQuest.Quest, endQuest.Detail, steps); return true;
        }

        // Priority 1: Unlocked Abilities (they cost resources but enable progression)
        foreach (var cadName in state.UnlockedCadences) {
            var cadence = cadences.All.FirstOrDefault(c => c.Name == cadName);
            if (cadence.Name == null || cadence.Abilities == null) continue;
            foreach (var unlock in cadence.Abilities) {
                string key = $"{cadName}:{unlock.Ability.Name}";
                if (state.UnlockedAbilities.Contains(key)) continue;
                if (unlock.Requirements != null && CanAffordEventually(state, unlock.Requirements)) {
                    ExecuteAbilityUnlock(state, unlock, steps); return true;
                }
            }
        }

        // Priority 2: Targetable Quests (Single first, then Recurring)
        var targetable = available.Where(q => !state.CompletedQuests.Contains(q.Item1.Name)).OrderByDescending(q => q.Item2.Type != QuestType.Recurring ? 1 : 0).ThenBy(q => q.Item2.DurationSeconds).ToList();
        
        foreach (var target in targetable) {
            if (CanAffordEventually(state, target.Item2.Requirements)) {
                ExecuteQuest(state, target.Item1, target.Item2, steps); return true;
            }
        }

        // Priority 3: Prerequisites for uncompleted single quests
        var uncompletedSingles = locations.All.SelectMany(l => l.Quests ?? []).Where(q => (questDetails[q].Type != QuestType.Recurring) && !state.CompletedQuests.Contains(q.Name)).ToList();
        foreach (var q in uncompletedSingles) {
            var prereqs = questUnlocks[q]; if (prereqs == null) continue;
            foreach (var pre in prereqs) {
                if (!state.CompletedQuests.Contains(pre.Name)) {
                    var match = available.FirstOrDefault(x => x.Quest.Name == pre.Name);
                    if (match.Quest.Name != null && CanAffordEventually(state, match.Detail.Requirements)) {
                        ExecuteQuest(state, match.Quest, match.Detail, steps); return true;
                    }
                }
            }
        }

        return false;
    }

    private void ExecuteAbilityUnlock(SimulationState state, CadenceUnlock unlock, int steps)
    {
        if (unlock.Requirements != null) {
            foreach (var req in unlock.Requirements) FarmResource(state, req.Item, req.Quantity, steps);
            foreach (var req in unlock.Requirements) SubtractFromInventory(state, req.Item.Name, req.Quantity);
        }
        state.CurrentTime += 30.0 * Math.Pow(0.75, (state.CurrentStats.GetValueOrDefault(unlock.PrimaryStat ?? "Magic", 10) - 10) / 10.0);
        state.UnlockedAbilities.Add($"{unlock.CadenceName}:{unlock.Ability.Name}");
        Console.WriteLine($"[DEBUG] Unlocked Ability: {unlock.CadenceName}:{unlock.Ability.Name} at {state.CurrentTime/60.0:F1}m");
        UpdateStats(state, steps);
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
        if (requirements == null) return true;
        foreach (var req in requirements) {
            if (req.Item.Name == null) continue;
            if (GetFromInventory(state, req.Item.Name) >= req.Quantity) continue;
            var source = GetBestSource(state, req.Item);
            if (source.Quest?.Name == null && source.Ability?.Name == null) return false;
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

        // HEURISTIC: Prioritize quests that provide items needed for other available (but currently unaffordable) quests
        var neededItems = available.Where(q => !state.CompletedQuests.Contains(q.Item1.Name))
            .SelectMany(q => q.Item2.Requirements ?? [])
            .Where(req => GetFromInventory(state, req.Item.Name) < req.Quantity)
            .Select(req => req.Item.Name)
            .Distinct().ToHashSet();

        return available.OrderByDescending(q => q.Item1.Name == END_QUEST ? 1000 : 0)
                        .ThenByDescending(q => q.Item2.Rewards?.Any(r => neededItems.Contains(r.Item.Name)) ?? false ? 100 : 0)
                        .ThenBy(q => q.Item2.DurationSeconds)
                        .ToList();
        }
    private void ExecuteQuest(SimulationState state, Quest q, QuestDetail detail, int steps)
    {
        if (detail.Requirements != null) {
            foreach (var req in detail.Requirements) FarmResource(state, req.Item, req.Quantity, steps);
            foreach (var req in detail.Requirements) SubtractFromInventory(state, req.Item.Name, req.Quantity);
        }
        state.CurrentTime += detail.DurationSeconds * Math.Pow(0.75, (state.CurrentStats.GetValueOrDefault(detail.PrimaryStat ?? "Vitality", 10) - 10) / 10.0);
        state.CompletedQuests.Add(q.Name);
        if (detail.Rewards != null) foreach (var rew in detail.Rewards) state.Inventory[rew.Item.Name] = GetFromInventory(state, rew.Item.Name) + rew.Quantity;
        var cads = questToCadenceUnlocks[q];
        if (cads != null) foreach (var cad in cads) state.UnlockedCadences.Add(cad.Name);
        UpdateStats(state, steps);
    }

    private bool FarmResource(SimulationState state, Item item, long quantityNeeded, int steps)
    {
        if (_farmingStack.Contains(item.Name)) return false;
        _farmingStack.Add(item.Name);
        try {
            long current = GetFromInventory(state, item.Name);
            if (current >= quantityNeeded) return true;
            var source = GetBestSource(state, item);
            var sq = source.Quest; var sd = source.Detail;
            if (sq?.Name != null && sd?.DurationSeconds > 0) {
                var rewards = ((QuestDetail)sd).Rewards;
                if (rewards != null) {
                    var reward = rewards.First(r => r.Item == item);
                    int runs = (int)Math.Min(1000, Math.Ceiling((double)(quantityNeeded - current) / reward.Quantity));
                    for (int i = 0; i < runs; i++) ExecuteQuest(state, (Quest)sq, (QuestDetail)sd, steps);
                    return GetFromInventory(state, item.Name) >= quantityNeeded;
                }
            } else if (source.Ability?.Name != null && source.Recipes != null) {
                var recipe = source.Recipes.First(x => x.Value.OutputItem == item);
                int runs = (int)Math.Min(1000, Math.Ceiling((double)(quantityNeeded - current) / recipe.Value.OutputQuantity));
                for (int i = 0; i < runs; i++) ExecuteRefinement(state, (CadenceAbility)source.Ability, (string)source.PrimaryStat!, recipe.Key, recipe.Value, steps);
                return GetFromInventory(state, item.Name) >= quantityNeeded;
            }
            return false;
        } finally { _farmingStack.Remove(item.Name); }
    }

    private void ExecuteRefinement(SimulationState state, CadenceAbility ability, string primaryStat, Item input, Recipe recipe, int steps)
    {
        FarmResource(state, input, recipe.InputQuantity, steps);
        SubtractFromInventory(state, input.Name, recipe.InputQuantity);
        state.CurrentTime += 15.0 * Math.Pow(0.75, (state.CurrentStats.GetValueOrDefault(primaryStat, 10) - 10) / 10.0);
        state.Inventory[recipe.OutputItem.Name] = GetFromInventory(state, recipe.OutputItem.Name) + recipe.OutputQuantity;
        UpdateStats(state, steps);
    }

    private ActivitySource GetBestSource(SimulationState state, Item item)
    {
        // Check Recurring Quests first
        var recurring = locations.All.SelectMany(l => l.Quests ?? Array.Empty<Quest>()).Where(q => {
            var loc = locations.All.First(x => x.Quests != null && x.Quests.Contains(q));
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) return false;
            if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) return false;
            var d = questDetails[q]; return d.Type == QuestType.Recurring && (d.Rewards?.Any(r => r.Item == item) ?? false);
        }).OrderByDescending(q => (double)(questDetails[q].Rewards?.First(r => r.Item == item).Quantity ?? 0) / questDetails[q].DurationSeconds).FirstOrDefault();
        if (recurring.Name != null) return new ActivitySource { Quest = recurring, Detail = questDetails[recurring] };

        // Check Single Quests (if not completed)
        var singles = locations.All.SelectMany(l => l.Quests ?? Array.Empty<Quest>()).Where(q => {
            var loc = locations.All.First(x => x.Quests != null && x.Quests.Contains(q));
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) return false;
            if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) return false;
            var d = questDetails[q]; return d.Type != QuestType.Recurring && !state.CompletedQuests.Contains(q.Name) && (d.Rewards?.Any(r => r.Item == item) ?? false);
        }).OrderByDescending(q => (double)(questDetails[q].Rewards?.First(r => r.Item == item).Quantity ?? 0) / questDetails[q].DurationSeconds).FirstOrDefault();
        if (singles.Name != null) return new ActivitySource { Quest = singles, Detail = questDetails[singles] };

        var refMatch = refinements.ByKey.Where(r => state.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")) && (r.Value.Recipes?.Values.Any(rec => rec.OutputItem == item) ?? false)).FirstOrDefault();
        if (refMatch.Key.Name != null) return new ActivitySource { Ability = refMatch.Key, PrimaryStat = refMatch.Value.PrimaryStat, Recipes = refMatch.Value.Recipes };
        return new ActivitySource();
    }

    private void UpdateStats(SimulationState state, int steps)
    {
        // Update Magic Capacity first
        int cap = 30;
        foreach (var abilityKey in state.UnlockedAbilities)
        {
            var parts = abilityKey.Split(':');
            if (parts.Length < 2) continue;
            var cadenceName = parts[0];
            var abilityName = parts[1];
            
            var cadence = cadences.All.FirstOrDefault(c => c.Name == cadenceName);
            var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == abilityName);
            if (unlock.Ability.Effects != null)
            {
                foreach (var effect in unlock.Ability.Effects)
                {
                    if (effect.Type == EffectType.MagicCapacity)
                    {
                        cap = Math.Max(cap, effect.Value);
                    }
                }
            }
        }
        state.MagicCapacity = cap;

        if (state.CurrentStats.GetValueOrDefault("Strength", 0) >= 60) state.UnlockedCadences.Add("Geologist");
        if (state.CurrentStats.GetValueOrDefault("Speed", 0) >= 60) state.UnlockedCadences.Add("Tide-Caller");
        if (state.CurrentStats.GetValueOrDefault("Vitality", 0) >= 60) state.UnlockedCadences.Add("The Sentinel");
        if (state.CurrentStats.GetValueOrDefault("Magic", 0) >= 100) state.UnlockedCadences.Add("Scholar");
        if (state.CurrentStats.GetValueOrDefault("Strength", 0) >= 100 && state.CurrentStats.GetValueOrDefault("Speed", 0) >= 100) state.UnlockedCadences.Add("Slayer");

        foreach (var stat in stats.All) {
            string jAbil = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
            if (state.UnlockedAbilities.Any(ua => ua.EndsWith($":{jAbil}"))) {
                // Proactively try to farm the BEST spell we can eventually afford for this stat
                var potentialSpells = items.All.Where(i => i.ItemType == ItemType.Spell)
                    .Select(i => (Item: i, Augment: statAugments[i].FirstOrDefault(a => a.Stat.Name == stat.Name)))
                    .Where(x => x.Augment.Stat.Name != null).OrderByDescending(x => x.Augment.ModifierAtFull).ToList();

                foreach (var spell in potentialSpells) {
                    // If we can eventually afford it, farm it (just 1)
                    var source = GetBestSource(state, spell.Item);
                    if (source.Quest?.Name != null || source.Ability?.Name != null) {
                        FarmResource(state, spell.Item, 1, steps);
                        if (GetFromInventory(state, spell.Item.Name) > 0) break; // Got it
                    }
                }

                var best = items.All.Where(i => i.ItemType == ItemType.Spell && GetFromInventory(state, i.Name) > 0)
                    .Select(i => (Item: i, Augment: statAugments[i].FirstOrDefault(a => a.Stat.Name == stat.Name)))
                    .Where(x => x.Augment.Stat.Name != null).OrderByDescending(x => x.Augment.ModifierAtFull).FirstOrDefault();
                if (best.Item.Name != null) {
                    int val = 10 + (int)(state.MagicCapacity * (best.Augment.ModifierAtFull / 100.0));
                    state.CurrentStats[stat.Name] = Math.Max(state.CurrentStats.GetValueOrDefault(stat.Name, 25), Math.Min(255, val));
                } else {
                    // Log if we have J-Stat but no spell for it
                    if (steps > 0 && steps % 100 == 0) Console.WriteLine($"[DEBUG] Character has {jAbil} but no spells in inventory for it.");
                }
            }
        }
    }
}
