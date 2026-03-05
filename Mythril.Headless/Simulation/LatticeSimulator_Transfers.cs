using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class LatticeSimulator
{
    private GameState ApplyTransfers(GameState state)
    {
        var itemMap = items.All.ToDictionary(i => i.Name);
        var questMap = quests.All.ToDictionary(q => q.Name);
        var cadenceMap = cadences.All.ToDictionary(c => c.Name);

        var next = state;
        next = Join(next, ApplyQuestTransfers(state, questMap));
        next = Join(next, ApplyRefinementTransfers(state));
        next = Join(next, ApplyAbilityUnlockTransfers(state, cadenceMap));
        next = Join(next, ApplyStatTransfers(state, itemMap));
        next = Join(next, ApplyHiddenCadenceTransfers(state));
        return next;
    }

    private GameState ApplyQuestTransfers(GameState state, Dictionary<string, Quest> questMap)
    {
        var questTimes = state.QuestTime.ToBuilder();
        var resourceTimes = state.ResourceTime.ToBuilder();
        var cadenceUnlocks = state.UnlockedCadences.ToBuilder();

        foreach (var loc in locations.All)
        {
            double locTime = string.IsNullOrEmpty(loc.RequiredQuest) ? 0 : state.QuestTime.GetValueOrDefault(loc.RequiredQuest, double.PositiveInfinity);
            if (locTime == double.PositiveInfinity) continue;

            foreach (var quest in loc.Quests)
            {
                var detail = questDetails[quest];
                
                double prereqTime = 0;
                bool prereqsMet = true;
                foreach (var reqQ in questUnlocks[quest])
                {
                    double t = state.QuestTime.GetValueOrDefault(reqQ.Name, double.PositiveInfinity);
                    if (t == double.PositiveInfinity) { prereqsMet = false; break; }
                    prereqTime = Math.Max(prereqTime, t);
                }
                if (!prereqsMet) continue;

                double itemTime = 0;
                bool itemsMet = true;
                foreach (var reqI in detail.Requirements)
                {
                    double t = state.ResourceTime.GetValueOrDefault(reqI.Item.Name, double.PositiveInfinity);
                    if (t == double.PositiveInfinity) { itemsMet = false; break; }
                    itemTime = Math.Max(itemTime, t);
                }
                if (!itemsMet) continue;

                bool statsMet = true;
                if (detail.RequiredStats != null)
                {
                    foreach (var reqS in detail.RequiredStats)
                    {
                        if (state.StatMax.GetValueOrDefault(reqS.Key, 0) < reqS.Value) { statsMet = false; break; }
                    }
                }
                if (!statsMet) continue;

                double startTime = Math.Max(locTime, Math.Max(prereqTime, itemTime));
                double duration = detail.DurationSeconds / (1.0 + (state.StatMax.GetValueOrDefault(detail.PrimaryStat, 10) / 100.0));
                double completionTime = startTime + duration;

                if (completionTime < questTimes.GetValueOrDefault(quest.Name, double.PositiveInfinity))
                {
                    questTimes[quest.Name] = completionTime;
                }

                foreach (var reward in detail.Rewards)
                {
                    if (completionTime < resourceTimes.GetValueOrDefault(reward.Item.Name, double.PositiveInfinity))
                    {
                        resourceTimes[reward.Item.Name] = completionTime;
                    }
                }

                foreach (var cad in questToCadenceUnlocks[quest])
                {
                    cadenceUnlocks.Add(cad.Name);
                }
            }
        }

        return state with { 
            QuestTime = questTimes.ToImmutable(), 
            ResourceTime = resourceTimes.ToImmutable(),
            UnlockedCadences = cadenceUnlocks.ToImmutable()
        };
    }

    private GameState ApplyRefinementTransfers(GameState state)
    {
        var resourceTimes = state.ResourceTime.ToBuilder();

        foreach (var refinementKvp in refinements.ByKey)
        {
            var ability = refinementKvp.Key;
            if (!state.UnlockedAbilities.Any(ua => ua.EndsWith($":{ability.Name}"))) continue;

            foreach (var recipeKvp in refinementKvp.Value.Recipes)
            {
                var inputItem = recipeKvp.Key;
                var recipe = recipeKvp.Value;

                double inputTime = state.ResourceTime.GetValueOrDefault(inputItem.Name, double.PositiveInfinity);
                if (inputTime == double.PositiveInfinity) continue;

                double duration = 15.0 / (1.0 + (state.StatMax.GetValueOrDefault(refinementKvp.Value.PrimaryStat, 10) / 100.0));
                double outputTime = inputTime + duration;

                if (outputTime < resourceTimes.GetValueOrDefault(recipe.OutputItem.Name, double.PositiveInfinity))
                {
                    resourceTimes[recipe.OutputItem.Name] = outputTime;
                }
            }
        }

        return state with { ResourceTime = resourceTimes.ToImmutable() };
    }

    private GameState ApplyAbilityUnlockTransfers(GameState state, Dictionary<string, Cadence> cadenceMap)
    {
        var abilities = state.UnlockedAbilities.ToBuilder();
        int newCapacity = state.MagicCapacity;

        foreach (var cadenceName in state.UnlockedCadences)
        {
            if (!cadenceMap.TryGetValue(cadenceName, out var cadence)) continue;
            foreach (var unlock in cadence.Abilities)
            {
                string key = $"{cadence.Name}:{unlock.Ability.Name}";
                
                // Always check metadata for capacity, even if already unlocked
                if (state.UnlockedAbilities.Contains(key))
                {
                    if (unlock.Ability.Metadata != null && unlock.Ability.Metadata.TryGetValue("MagicCapacity", out var capStr) && int.TryParse(capStr, out var capVal))
                    {
                        newCapacity = Math.Max(newCapacity, capVal);
                    }
                    continue;
                }

                double costTime = 0;
                bool canAfford = true;
                foreach (var req in unlock.Requirements)
                {
                    double t = state.ResourceTime.GetValueOrDefault(req.Item.Name, double.PositiveInfinity);
                    if (t == double.PositiveInfinity) { canAfford = false; break; }
                    costTime = Math.Max(costTime, t);
                }

                if (canAfford)
                {
                    abilities.Add(key);
                    if (unlock.Ability.Metadata != null && unlock.Ability.Metadata.TryGetValue("MagicCapacity", out var capStr) && int.TryParse(capStr, out var capVal))
                    {
                        if (capVal > newCapacity)
                        {
                            Console.WriteLine($"DEBUG: Capacity increasing {newCapacity} -> {capVal} via {key}");
                            newCapacity = capVal;
                        }
                    }
                }
            }
        }

        return state with { UnlockedAbilities = abilities.ToImmutable(), MagicCapacity = newCapacity };
    }

    private GameState ApplyStatTransfers(GameState state, Dictionary<string, Item> itemMap)
    {
        var statsMax = state.StatMax.ToBuilder();

        foreach (var stat in stats.All)
        {
            int bestVal = state.StatMax.GetValueOrDefault(stat.Name, 10);
            foreach (var itemKvp in state.ResourceTime)
            {
                if (itemKvp.Value == double.PositiveInfinity) continue;
                
                if (itemMap.TryGetValue(itemKvp.Key, out var item))
                {
                    if (item.ItemType != ItemType.Spell) continue;

                    string abilityName = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
                    if (state.UnlockedAbilities.Any(ua => ua.EndsWith($":{abilityName}")))
                    {
                        var augment = statAugments[item].FirstOrDefault(a => a.Stat.Name == stat.Name);
                        int val = 10 + (int)(state.MagicCapacity * (augment.Stat.Name != null ? augment.ModifierAtFull / 100.0 : 0.1));
                        bestVal = Math.Max(bestVal, Math.Min(255, val));
                    }
                }
            }
            statsMax[stat.Name] = bestVal;
        }

        return state with { StatMax = statsMax.ToImmutable() };
    }

    private GameState ApplyHiddenCadenceTransfers(GameState state)
    {
        var cadenceUnlocks = state.UnlockedCadences.ToBuilder();

        if (state.StatMax.GetValueOrDefault("Strength", 0) >= 60) cadenceUnlocks.Add("Geologist");
        if (state.StatMax.GetValueOrDefault("Speed", 0) >= 60) cadenceUnlocks.Add("Tide-Caller");
        if (state.StatMax.GetValueOrDefault("Magic", 0) >= 100) cadenceUnlocks.Add("Scholar");
        if (state.StatMax.GetValueOrDefault("Strength", 0) >= 100 && state.StatMax.GetValueOrDefault("Speed", 0) >= 100) cadenceUnlocks.Add("Slayer");

        return state with { UnlockedCadences = cadenceUnlocks.ToImmutable() };
    }
}
