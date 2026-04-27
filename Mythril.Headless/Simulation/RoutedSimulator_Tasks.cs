using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class RoutedSimulator
{
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
        var recurring = locations.All.SelectMany(l => l.Quests ?? []).Where(q =>
        {
            var loc = locations.All.First(x => x.Quests?.Contains(q) == true);
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) return false;
            if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) return false;
            var d = questDetails[q]; return d.Type == QuestType.Recurring && (d.Rewards?.Any(r => r.Item == item) ?? false);
        }).OrderByDescending(q => (double)(questDetails[q].Rewards?.First(r => r.Item == item).Quantity ?? 0) / questDetails[q].DurationSeconds).FirstOrDefault();
        if (recurring.Name != null) return new ActivitySource { Quest = recurring, Detail = questDetails[recurring] };

        // Check Single Quests (if not completed)
        var singles = locations.All.SelectMany(l => l.Quests ?? []).Where(q =>
        {
            var loc = locations.All.First(x => x.Quests?.Contains(q) == true);
            if (!string.IsNullOrEmpty(loc.RequiredQuest) && !state.CompletedQuests.Contains(loc.RequiredQuest)) return false;
            if (questUnlocks[q]?.Any(req => !state.CompletedQuests.Contains(req.Name)) ?? false) return false;
            var d = questDetails[q]; return d.Type != QuestType.Recurring && !state.CompletedQuests.Contains(q.Name) && (d.Rewards?.Any(r => r.Item == item) ?? false);
        }).OrderByDescending(q => (double)(questDetails[q].Rewards?.First(r => r.Item == item).Quantity ?? 0) / questDetails[q].DurationSeconds).FirstOrDefault();
        if (singles.Name != null) return new ActivitySource { Quest = singles, Detail = questDetails[singles] };

        var refMatch = refinements.ByKey.FirstOrDefault(r => state.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")) && (r.Value.Recipes?.Values.Any(rec => rec.OutputItem == item) ?? false));
        if (refMatch.Key.Name != null) return new ActivitySource { Ability = refMatch.Key, PrimaryStat = refMatch.Value.PrimaryStat, Recipes = refMatch.Value.Recipes };
        return new ActivitySource();
    }

    private void UpdateStats(SimulationState state, int steps)
    {
        // Update Magic Capacity first
        var cap = 30;
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

        foreach (var stat in stats.All)
        {
            var jAbil = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
            if (state.UnlockedAbilities.Any(ua => ua.EndsWith($":{jAbil}")))
            {
                // Proactively try to farm the BEST spell we can eventually afford for this stat
                var potentialSpells = items.All.Where(i => i.ItemType == ItemType.Spell)
                    .Select(i => (Item: i, Augment: statAugments[i].FirstOrDefault(a => a.Stat.Name == stat.Name)))
                    .Where(x => x.Augment.Stat.Name != null).OrderByDescending(x => x.Augment.ModifierAtFull).ToList();

                foreach (var spell in potentialSpells)
                {
                    // If we can eventually afford it, farm it (just 1)
                    var source = GetBestSource(state, spell.Item);
                    if (source.Quest?.Name != null || source.Ability?.Name != null)
                    {
                        FarmResource(state, spell.Item, 1, steps);
                        if (GetFromInventory(state, spell.Item.Name) > 0) break; // Got it
                    }
                }

                var best = items.All.Where(i => i.ItemType == ItemType.Spell && GetFromInventory(state, i.Name) > 0)
                    .Select(i => (Item: i, Augment: statAugments[i].FirstOrDefault(a => a.Stat.Name == stat.Name)))
                    .Where(x => x.Augment.Stat.Name != null).OrderByDescending(x => x.Augment.ModifierAtFull).FirstOrDefault();
                if (best.Item.Name != null)
                {
                    var val = 10 + (int)(state.MagicCapacity * (best.Augment.ModifierAtFull / 100.0));
                    state.CurrentStats[stat.Name] = Math.Max(state.CurrentStats.GetValueOrDefault(stat.Name, 25), Math.Min(255, val));
                }
                else
                {
                    // Log if we have J-Stat but no spell for it
                    if (steps > 0 && steps % 100 == 0) Console.WriteLine($"[DEBUG] Character has {jAbil} but no spells in inventory for it.");
                }
            }
        }
    }
}