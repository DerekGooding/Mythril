using Mythril.Data;
using System.Collections.Immutable;

namespace Mythril.Headless.Simulation;

public partial class LatticeSimulator
{
    private (bool, GameState) UpdateQuest(string name, GameState state)
    {
        var quest = quests.All.First(q => q.Name == name);
        var detail = questDetails[quest];

        // 1. Location requirement
        double locTime = 0;
        var loc = locations.All.FirstOrDefault(l => l.Quests.Contains(quest));
        if (!string.IsNullOrEmpty(loc.Name) && !string.IsNullOrEmpty(loc.RequiredQuest))
        {
            locTime = state.QuestTime.GetValueOrDefault(loc.RequiredQuest, double.PositiveInfinity);
        }
        if (locTime == double.PositiveInfinity) return (false, state);

        // 2. Prerequisites
        double prereqTime = 0;
        foreach (var reqQ in questUnlocks[quest])
        {
            var t = state.QuestTime.GetValueOrDefault(reqQ.Name, double.PositiveInfinity);
            if (t == double.PositiveInfinity) return (false, state);
            prereqTime = Math.Max(prereqTime, t);
        }

        // 3. Item requirements
        double itemTime = 0;
        foreach (var reqI in detail.Requirements)
        {
            var t = state.ResourceTime.GetValueOrDefault(reqI.Item.Name, double.PositiveInfinity);
            if (t == double.PositiveInfinity) return (false, state);
            itemTime = Math.Max(itemTime, t);
        }

        // 4. Stat requirements
        if (detail.RequiredStats != null)
        {
            foreach (var reqS in detail.RequiredStats)
            {
                if (state.StatMax.GetValueOrDefault(reqS.Key, 0) < reqS.Value) return (false, state);
            }
        }

        var startTime = Math.Max(locTime, Math.Max(prereqTime, itemTime));
        double statValue = state.StatMax.GetValueOrDefault(detail.PrimaryStat, 10);
        var duration = detail.DurationSeconds * Math.Pow(0.75, (statValue - 10) / 10.0);
        var completionTime = startTime + duration;

        var changed = false;
        var nextQuestTime = state.QuestTime;
        if (completionTime < state.QuestTime.GetValueOrDefault(name, double.PositiveInfinity))
        {
            nextQuestTime = state.QuestTime.SetItem(name, completionTime);
            changed = true;
        }

        var nextResourceTime = state.ResourceTime;
        foreach (var reward in detail.Rewards)
        {
            if (completionTime < state.ResourceTime.GetValueOrDefault(reward.Item.Name, double.PositiveInfinity))
            {
                nextResourceTime = nextResourceTime.SetItem(reward.Item.Name, completionTime);
                changed = true;
            }
        }

        var nextCadences = state.UnlockedCadences;
        foreach (var cad in questToCadenceUnlocks[quest])
        {
            if (!state.UnlockedCadences.Contains(cad.Name))
            {
                nextCadences = nextCadences.Add(cad.Name);
                changed = true;
            }
        }

        var nextStatMax = state.StatMax;
        // Only apply permanent stat boosts when the quest is FIRST reached (transition from infinity to finite time)
        if (state.QuestTime.GetValueOrDefault(name, double.PositiveInfinity) == double.PositiveInfinity && completionTime < double.PositiveInfinity)
        {
            if (detail.Effects != null)
            {
                foreach (var effect in detail.Effects)
                {
                    if (effect.Type == EffectType.StatBoost && !string.IsNullOrEmpty(effect.Target))
                    {
                        var current = nextStatMax.GetValueOrDefault(effect.Target, 10);
                        nextStatMax = nextStatMax.SetItem(effect.Target, current + effect.Value);
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            return (true, state with
            {
                QuestTime = nextQuestTime,
                ResourceTime = nextResourceTime,
                UnlockedCadences = nextCadences,
                StatMax = nextStatMax
            });
        }

        return (false, state);
    }

    private (bool, GameState) UpdateRefinement(string name, GameState state)
    {
        // Name format: "AbilityName:InputItemName"
        var parts = name.Split(':');
        var abilityName = parts[0];
        var inputItemName = parts[1];

        var ability = refinements.ByKey.Keys.First(a => a.Name == abilityName);
        var (PrimaryStat, Recipes) = refinements.ByKey[ability];
        var inputItem = items.All.First(i => i.Name == inputItemName);
        var recipe = Recipes[inputItem];

        if (!state.UnlockedAbilities.Any(ua => ua.EndsWith($":{abilityName}"))) return (false, state);

        var inputTime = state.ResourceTime.GetValueOrDefault(inputItemName, double.PositiveInfinity);
        if (inputTime == double.PositiveInfinity) return (false, state);

        double statValue = state.StatMax.GetValueOrDefault(PrimaryStat, 10);
        var duration = 15.0 * Math.Pow(0.75, (statValue - 10) / 10.0);
        var outputTime = inputTime + duration;

        if (outputTime < state.ResourceTime.GetValueOrDefault(recipe.OutputItem.Name, double.PositiveInfinity))
        {
            return (true, state with
            {
                ResourceTime = state.ResourceTime.SetItem(recipe.OutputItem.Name, outputTime)
            });
        }

        return (false, state);
    }

    private (bool, GameState) UpdateAbility(string name, GameState state)
    {
        var changed = false;
        var nextAbilities = state.UnlockedAbilities;
        var nextCapacity = state.MagicCapacity;

        foreach (var cadence in cadences.All)
        {
            var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == name);
            if (string.IsNullOrEmpty(unlock.Ability.Name)) continue;

            var key = $"{cadence.Name}:{unlock.Ability.Name}";
            if (!state.UnlockedCadences.Contains(cadence.Name)) continue;

            // metadata capacity check
            if (unlock.Ability.Effects != null)
            {
                foreach (var effect in unlock.Ability.Effects)
                {
                    if (effect.Type == EffectType.MagicCapacity)
                    {
                        if (effect.Value > nextCapacity)
                        {
                            nextCapacity = effect.Value;
                            changed = true;
                        }
                    }
                }
            }

            if (state.UnlockedAbilities.Contains(key)) continue;

            var canAfford = true;
            foreach (var req in unlock.Requirements)
            {
                if (state.ResourceTime.GetValueOrDefault(req.Item.Name, double.PositiveInfinity) == double.PositiveInfinity)
                {
                    canAfford = false;
                    break;
                }
            }

            if (canAfford)
            {
                nextAbilities = nextAbilities.Add(key);
                changed = true;
            }
        }

        if (changed)
        {
            return (true, state with { UnlockedAbilities = nextAbilities, MagicCapacity = nextCapacity });
        }

        return (false, state);
    }
}