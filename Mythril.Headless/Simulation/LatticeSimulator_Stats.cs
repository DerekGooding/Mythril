using Mythril.Data;
using System.Collections.Immutable;

namespace Mythril.Headless.Simulation;

public partial class LatticeSimulator
{
    private (bool, GameState) UpdateStat(string name, GameState state)
    {
        var bestVal = state.StatMax.GetValueOrDefault(name, 10);
        var abilityName = name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + name };

        var hasAbility = state.UnlockedAbilities.Any(ua => ua.EndsWith($":{abilityName}"));
        if (!hasAbility) return (false, state);

        foreach (var itemKvp in state.ResourceTime)
        {
            if (itemKvp.Value == double.PositiveInfinity) continue;

            var item = items.All.First(i => i.Name == itemKvp.Key);
            if (item.ItemType != ItemType.Spell) continue;

            var augments = statAugments[item];
            var augment = augments.FirstOrDefault(a => a.Stat.Name == name);
            if (augment.Stat.Name != null)
            {
                var val = 10 + (int)(state.MagicCapacity * (augment.ModifierAtFull / 100.0));
                bestVal = Math.Max(bestVal, Math.Min(255, val));
            }
        }

        if (bestVal > state.StatMax.GetValueOrDefault(name, 10))
        {
            return (true, state with { StatMax = state.StatMax.SetItem(name, bestVal) });
        }

        return (false, state);
    }

    private (bool, GameState) UpdateCadence(string name, GameState state)
    {
        if (name == "HIDDEN")
        {
            var nextCadences = state.UnlockedCadences;
            var changed = false;

            void Check(string cad, bool condition)
            {
                if (condition && !nextCadences.Contains(cad))
                {
                    nextCadences = nextCadences.Add(cad);
                    changed = true;
                }
            }

            Check("Geologist", state.StatMax.GetValueOrDefault("Strength", 0) >= 60);
            Check("Tide-Caller", state.StatMax.GetValueOrDefault("Speed", 0) >= 60);
            Check("The Sentinel", state.StatMax.GetValueOrDefault("Vitality", 0) >= 60);
            Check("Scholar", state.StatMax.GetValueOrDefault("Magic", 0) >= 100);
            Check("Slayer", state.StatMax.GetValueOrDefault("Strength", 0) >= 100 && state.StatMax.GetValueOrDefault("Speed", 0) >= 100);

            if (changed) return (true, state with { UnlockedCadences = nextCadences });
        }
        else
        {
            // Quest-based cadences are handled in UpdateQuest
        }

        return (false, state);
    }

    private (bool, GameState) UpdateCapacity(GameState state)
    {
        var bestCap = 30;
        foreach (var abilityKey in state.UnlockedAbilities)
        {
            var parts = abilityKey.Split(':');
            var cadenceName = parts[0];
            var abilityName = parts[1];

            var cadence = cadences.All.FirstOrDefault(c => c.Name == cadenceName);
            if (cadence.Name == null) continue;
            var unlock = cadence.Abilities.FirstOrDefault(a => a.Ability.Name == abilityName);
            if (unlock.Ability.Name == null) continue;

            if (unlock.Ability.Effects != null)
            {
                foreach (var effect in unlock.Ability.Effects)
                {
                    if (effect.Type == EffectType.MagicCapacity)
                    {
                        bestCap = Math.Max(bestCap, effect.Value);
                    }
                }
            }
        }

        if (bestCap > state.MagicCapacity)
        {
            return (true, state with { MagicCapacity = bestCap });
        }

        return (false, state);
    }
}