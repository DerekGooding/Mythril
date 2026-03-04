using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly HashSet<string> _infiniteResources = [];
    private readonly Dictionary<string, int> _oneTimeResources = [];
    private readonly HashSet<string> _completedQuests = [];
    private readonly HashSet<string> _unlockedCadences = [];
    private readonly HashSet<string> _unlockedAbilities = []; // "CadenceName:AbilityName"
    private readonly Dictionary<string, int> _maxStats = stats.All.ToDictionary(s => s.Name, _ => 10);
    private int _magicCapacity = 30;

    private readonly Dictionary<string, double> _minTimeToReachResource = [];
    private readonly Dictionary<string, double> _minTimeToReachQuest = [];

    public void Run()
    {
        bool changed = true;
        int iteration = 0;

        foreach (var item in items.All) _minTimeToReachResource[item.Name] = double.PositiveInfinity;
        foreach (var quest in quests.All) _minTimeToReachQuest[quest.Name] = double.PositiveInfinity;

        while (changed && iteration < 1000)
        {
            changed = false;
            iteration++;

            foreach (var loc in locations.All)
            {
                if (string.IsNullOrEmpty(loc.RequiredQuest) || _completedQuests.Contains(loc.RequiredQuest))
                {
                    foreach (var quest in loc.Quests)
                    {
                        if (CanCompleteQuest(quest, out double time))
                        {
                            if (!_completedQuests.Contains(quest.Name)) { _completedQuests.Add(quest.Name); changed = true; }
                            if (time < _minTimeToReachQuest[quest.Name])
                            {
                                _minTimeToReachQuest[quest.Name] = time;
                                changed = true;
                                var detail = questDetails[quest];
                                foreach (var reward in detail.Rewards) UpdateResourceReachability(reward.Item.Name, reward.Quantity, detail.Type == QuestType.Recurring, time);
                                foreach (var cadence in questToCadenceUnlocks[quest]) { if (!_unlockedCadences.Contains(cadence.Name)) { _unlockedCadences.Add(cadence.Name); changed = true; } }
                            }
                        }
                    }
                }
            }

            foreach (var cadenceName in _unlockedCadences)
            {
                var cadence = cadences.All.First(c => c.Name == cadenceName);
                foreach (var unlock in cadence.Abilities)
                {
                    string abilityKey = $"{cadence.Name}:{unlock.Ability.Name}";
                    if (!_unlockedAbilities.Contains(abilityKey) && CanAfford(unlock.Requirements, out double costTime))
                    {
                        _unlockedAbilities.Add(abilityKey); changed = true;
                        if (unlock.Ability.Name == "Magic Pocket I") _magicCapacity = Math.Max(_magicCapacity, 60);
                        if (unlock.Ability.Name == "Magic Pocket II") _magicCapacity = Math.Max(_magicCapacity, 100);
                    }
                }
            }

            foreach (var abilityKvp in refinements.ByKey)
            {
                if (_unlockedAbilities.Any(ua => ua.EndsWith($":{abilityKvp.Key.Name}")))
                {
                    foreach (var recipeKvp in abilityKvp.Value.Recipes)
                    {
                        if (IsResourceAvailable(recipeKvp.Key.Name, recipeKvp.Value.InputQuantity, out double inputTime))
                        {
                            double outputTime = inputTime + (15.0 / recipeKvp.Value.OutputQuantity); 
                            if (UpdateResourceReachability(recipeKvp.Value.OutputItem.Name, recipeKvp.Value.OutputQuantity, true, outputTime)) changed = true;
                        }
                    }
                }
            }

            if (UpdateStats()) changed = true;
        }
        GenerateReport();
    }

    private bool IsResourceAvailable(string name, int qty, out double time)
    {
        time = _minTimeToReachResource.GetValueOrDefault(name, double.PositiveInfinity);
        return _infiniteResources.Contains(name) || _oneTimeResources.GetValueOrDefault(name, 0) >= qty;
    }

    private bool CanAfford(ItemQuantity[] requirements, out double time)
    {
        time = 0;
        foreach (var req in requirements)
        {
            if (!IsResourceAvailable(req.Item.Name, req.Quantity, out double resTime)) { time = double.PositiveInfinity; return false; }
            time = Math.Max(time, resTime);
        }
        return true;
    }

    private bool CanCompleteQuest(Quest quest, out double time)
    {
        var detail = questDetails[quest];
        if (!CanAfford(detail.Requirements, out double costTime)) { time = double.PositiveInfinity; return false; }
        foreach (var reqQuest in questUnlocks[quest])
        {
            if (!_completedQuests.Contains(reqQuest.Name)) { time = double.PositiveInfinity; return false; }
            costTime = Math.Max(costTime, _minTimeToReachQuest[reqQuest.Name]);
        }
        if (detail.RequiredStats != null)
        {
            foreach (var statReq in detail.RequiredStats) if (_maxStats[statReq.Key] < statReq.Value) { time = double.PositiveInfinity; return false; }
        }
        double duration = detail.DurationSeconds / (1.0 + (_maxStats[detail.PrimaryStat] / 100.0));
        time = costTime + duration;
        return true;
    }

    private bool UpdateResourceReachability(string name, int qty, bool infinite, double time)
    {
        bool changed = false;
        if (infinite && !_infiniteResources.Contains(name)) { _infiniteResources.Add(name); changed = true; }
        int currentQty = _oneTimeResources.GetValueOrDefault(name, 0);
        if (currentQty < 9999) { _oneTimeResources[name] = currentQty + qty; changed = true; }
        if (time < _minTimeToReachResource[name]) { _minTimeToReachResource[name] = time; changed = true; }
        return changed;
    }

    private bool UpdateStats()
    {
        bool changed = false;
        foreach (var stat in stats.All)
        {
            int bestVal = 10;
            foreach (var magicName in _infiniteResources)
            {
                var item = items.All.First(i => i.Name == magicName);
                if (item.ItemType == ItemType.Spell)
                {
                    string abilityName = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
                    if (_unlockedAbilities.Any(ua => ua.EndsWith($":{abilityName}")))
                    {
                        var augment = statAugments[item].FirstOrDefault(a => a.Stat.Name == stat.Name);
                        bestVal = Math.Max(bestVal, 10 + (int)(_magicCapacity * (augment.Stat.Name != null ? augment.ModifierAtFull / 100.0 : 0.1)));
                    }
                }
            }
            if (bestVal > _maxStats[stat.Name]) { _maxStats[stat.Name] = bestVal; changed = true; }
        }
        return changed;
    }
}
