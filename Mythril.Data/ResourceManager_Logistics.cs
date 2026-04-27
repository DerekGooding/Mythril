namespace Mythril.Data;

public partial class ResourceManager
{
    public int GetTaskLimit(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        var maxLogistics = 0;
        foreach (var cadence in assigned)
        {
            foreach (var unlock in cadence.Abilities)
            {
                if (UnlockedAbilities.Contains($"{cadence.Name}:{unlock.Ability.Name}") && unlock.Ability.Effects != null)
                {
                    foreach (var effect in unlock.Ability.Effects)
                    {
                        if (effect.Type == EffectType.Logistics)
                        {
                            maxLogistics = Math.Max(maxLogistics, effect.Value);
                        }
                    }
                }
            }
        }
        return 1 + maxLogistics;
    }

    public void ReevaluateActiveQuests(Character character)
    {
        lock (_questLock)
        {
            // 1. Check requirement failures (stats, abilities)
            var active = ActiveQuests.Where(p => p.Character.Name == character.Name).OrderBy(p => p.StartTime).ToList();
            foreach (var progress in active.ToList())
            {
                if (!MeetsRequirements(progress.Item, character))
                {
                    CancelQuest(progress);
                    active.Remove(progress);
                }
            }

            // 2. Check task limit
            var limit = GetTaskLimit(character);
            while (active.Count > limit)
            {
                var toCancel = active.Last();
                CancelQuest(toCancel);
                active.Remove(toCancel);
            }
        }
    }

    public bool CanAutoQuest(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        foreach (var cadence in assigned)
        {
            foreach (var unlock in cadence.Abilities)
            {
                if (UnlockedAbilities.Contains($"{cadence.Name}:{unlock.Ability.Name}") && unlock.Ability.Effects != null)
                {
                    foreach (var effect in unlock.Ability.Effects)
                    {
                        if (effect.Type == EffectType.AutoQuest) return true;
                    }
                }
            }
        }
        return false;
    }

    public bool IsAutoQuestEnabled(Character character) => _gameStore.State.AutoQuestEnabled.GetValueOrDefault(character.Name);

    public void SetAutoQuestEnabled(Character character, bool enabled) => _gameStore.Dispatch(new ToggleAutoQuestAction(character.Name, enabled));

    public void ToggleAutoQuest(Character character)
    {
        var current = IsAutoQuestEnabled(character);
        SetAutoQuestEnabled(character, !current);
    }

    public int GetAutoQuestLimit(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        var maxAuto = 0;
        foreach (var cadence in assigned)
        {
            foreach (var unlock in cadence.Abilities)
            {
                if (UnlockedAbilities.Contains($"{cadence.Name}:{unlock.Ability.Name}") && unlock.Ability.Effects != null)
                {
                    foreach (var effect in unlock.Ability.Effects)
                    {
                        if (effect.Type == EffectType.AutoQuest)
                        {
                            maxAuto = Math.Max(maxAuto, effect.Value);
                        }
                    }
                }
            }
        }
        return maxAuto;
    }

    private void CheckAutoQuestTick()
    {
        foreach (var character in Characters)
        {
            if (IsAutoQuestEnabled(character))
            {
                var limit = GetTaskLimit(character);
                var autoLimit = GetAutoQuestLimit(character);
                var current = ActiveQuests.Count(p => p.Character.Name == character.Name);

                //TODO => Fix autoquest now that journal is gone. Need a new standalone mapping of last tasks
                if (current < limit)
                {
                    // // Find the last entry for this character in this session
                    // var lastEntry = Journal.FirstOrDefault(j => j.CharacterName == character.Name);
                    // if (lastEntry?.TaskName != null && !lastEntry.WasCancelled)
                    // {
                    //     // Check if we are allowed to restart in the next free slot
                    //     // If current = 0, we are filling slot 0. If current = 1, we are filling slot 1.
                    //     if (current >= autoLimit) continue;

                    //     // Check if it's a recurring quest or refinement
                    //     var quest = _quests.All.FirstOrDefault(q => q.Name == lastEntry.TaskName);
                    //     if (quest.Name != null)
                    //     {
                    //         var detail = _questDetails[quest];
                    //         if (detail.Type == QuestType.Recurring)
                    //         {
                    //             var questData = new QuestData(quest, detail);
                    //             if (CanAfford(questData, character))
                    //             {
                    //                 // Use -1.5 as initialSecondsElapsed to provide a "preparing" visual delay in the UI
                    //                 StartQuest(questData, character, -1.5);
                    //             }
                    //         }
                    //     }
                    //     else
                    //     {
                    //         // Check refinements
                    //         var refData = _refinements.ByKey.SelectMany(r => r.Value.Recipes.Select(rec => new RefinementData(r.Key, rec.Key, rec.Value, r.Value.PrimaryStat)))
                    //             .FirstOrDefault(rd => rd.Name == lastEntry.TaskName);

                    //         if (refData.Name != null && CanAfford(refData, character))
                    //         {
                    //             StartQuest(refData, character, -1.5);
                    //         }
                    //     }
                    // }
                }
            }
        }
    }
}