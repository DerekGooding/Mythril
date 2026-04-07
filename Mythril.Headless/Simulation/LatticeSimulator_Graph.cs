using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class LatticeSimulator
{
    private enum NodeType { Quest, Resource, Refinement, Ability, Stat, Cadence, MagicCapacity }
    private record WorklistItem(NodeType Type, string Name);

    private Dictionary<WorklistItem, List<WorklistItem>> _dependents = new();

    private void BuildDependencyGraph()
    {
        _dependents.Clear();
        void AddDep(WorklistItem trigger, WorklistItem dependent)
        {
            if (!_dependents.ContainsKey(trigger)) _dependents[trigger] = new();
            if (!_dependents[trigger].Contains(dependent)) _dependents[trigger].Add(dependent);
        }

        var itemMap = items.All.ToDictionary(i => i.Name);

        // 1. Quest Dependencies
        foreach (var quest in quests.All)
        {
            var detail = questDetails[quest];
            var qItem = new WorklistItem(NodeType.Quest, quest.Name);

            // Item -> Quest
            foreach (var req in detail.Requirements)
            {
                AddDep(new WorklistItem(NodeType.Resource, req.Item.Name), qItem);
            }

            // Stat -> Quest (Requirements and Scaling)
            AddDep(new WorklistItem(NodeType.Stat, detail.PrimaryStat), qItem);
            if (detail.RequiredStats != null)
            {
                foreach (var reqS in detail.RequiredStats.Keys)
                {
                    AddDep(new WorklistItem(NodeType.Stat, reqS), qItem);
                }
            }

            // Quest -> Quest (Prerequisites)
            foreach (var reqQ in questUnlocks[quest])
            {
                AddDep(new WorklistItem(NodeType.Quest, reqQ.Name), qItem);
            }

            // Quest -> Item (Rewards)
            foreach (var reward in detail.Rewards)
            {
                AddDep(qItem, new WorklistItem(NodeType.Resource, reward.Item.Name));
            }

            // Quest -> Cadence (Unlocks)
            foreach (var cad in questToCadenceUnlocks[quest])
            {
                AddDep(qItem, new WorklistItem(NodeType.Cadence, cad.Name));
            }

            // Location requirements (Quest -> Quest)
            foreach (var loc in locations.All)
            {
                if (loc.Quests.Contains(quest) && !string.IsNullOrEmpty(loc.RequiredQuest))
                {
                    AddDep(new WorklistItem(NodeType.Quest, loc.RequiredQuest), qItem);
                }
            }
        }

        // 2. Refinement Dependencies
        foreach (var refinementKvp in refinements.ByKey)
        {
            var ability = refinementKvp.Key;
            var aItem = new WorklistItem(NodeType.Ability, ability.Name);

            foreach (var recipeKvp in refinementKvp.Value.Recipes)
            {
                var inputItem = recipeKvp.Key;
                var recipe = recipeKvp.Value;
                var rItem = new WorklistItem(NodeType.Refinement, $"{ability.Name}:{inputItem.Name}");

                AddDep(aItem, rItem);
                AddDep(new WorklistItem(NodeType.Resource, inputItem.Name), rItem);
                AddDep(new WorklistItem(NodeType.Stat, refinementKvp.Value.PrimaryStat), rItem);
                AddDep(rItem, new WorklistItem(NodeType.Resource, recipe.OutputItem.Name));
            }
        }

        // 3. Ability Dependencies
        foreach (var cad in cadences.All)
        {
            var cItem = new WorklistItem(NodeType.Cadence, cad.Name);
            foreach (var unlock in cad.Abilities)
            {
                var aItem = new WorklistItem(NodeType.Ability, unlock.Ability.Name);
                AddDep(cItem, aItem);
                foreach (var req in unlock.Requirements)
                {
                    AddDep(new WorklistItem(NodeType.Resource, req.Item.Name), aItem);
                }

                // Ability -> Capacity
                if (unlock.Ability.Effects != null && unlock.Ability.Effects.Any(e => e.Type == EffectType.MagicCapacity))
                {
                    AddDep(aItem, new WorklistItem(NodeType.MagicCapacity, ""));
                }
            }
        }

        // 4. Stat Dependencies
        foreach (var stat in stats.All)
        {
            var sItem = new WorklistItem(NodeType.Stat, stat.Name);
            string abilityName = stat.Name switch { "Strength" => "J-Str", "Magic" => "J-Magic", "Vitality" => "J-Vit", "Speed" => "J-Speed", _ => "J-" + stat.Name };
            
            AddDep(new WorklistItem(NodeType.Ability, abilityName), sItem);
            AddDep(new WorklistItem(NodeType.MagicCapacity, ""), sItem);

            foreach (var item in items.All.Where(i => i.ItemType == ItemType.Spell))
            {
                AddDep(new WorklistItem(NodeType.Resource, item.Name), sItem);
            }

            // Hidden Cadence unlocks
            if (stat.Name == "Strength" || stat.Name == "Speed" || stat.Name == "Vitality" || stat.Name == "Magic")
            {
                AddDep(sItem, new WorklistItem(NodeType.Cadence, "HIDDEN"));
            }
        }
    }
}
