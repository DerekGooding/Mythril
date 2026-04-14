namespace Mythril.Data;

public partial class ResourceManager
{
    public int GetTaskLimit(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        int maxLogistics = 0;
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

    public void CancelExcessQuests(Character character)
    {
        lock (_questLock)
        {
            int limit = GetTaskLimit(character);
            var active = ActiveQuests.Where(p => p.Character.Name == character.Name).OrderBy(p => p.StartTime).ToList();
            
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
        bool current = IsAutoQuestEnabled(character);
        SetAutoQuestEnabled(character, !current);
    }

    private void CheckAutoQuestTick()
    {
        foreach (var character in Characters)
        {
            if (IsAutoQuestEnabled(character))
            {
                int limit = GetTaskLimit(character);
                int current = ActiveQuests.Count(p => p.Character.Name == character.Name);
                
                if (current < limit)
                {
                    // Find the last completed quest for this character in this session
                    var lastCompleted = Journal.FirstOrDefault(j => j.CharacterName == character.Name);
                    if (lastCompleted?.TaskName != null)
                    {
                        // Check if it's a recurring quest or refinement
                        var quest = _quests.All.FirstOrDefault(q => q.Name == lastCompleted.TaskName);
                        if (quest.Name != null)
                        {
                            var detail = _questDetails[quest];
                            if (detail.Type == QuestType.Recurring)
                            {
                                var questData = new QuestData(quest, detail);
                                if (CanAfford(questData, character))
                                {
                                    // Use -1.5 as initialSecondsElapsed to provide a "preparing" visual delay in the UI
                                    StartQuest(questData, character, -1.5);
                                }
                            }
                        }
                        else
                        {
                            // Check refinements
                            var refData = _refinements.ByKey.SelectMany(r => r.Value.Recipes.Select(rec => new RefinementData(r.Key, rec.Key, rec.Value, r.Value.PrimaryStat)))
                                .FirstOrDefault(rd => rd.Name == lastCompleted.TaskName);
                            
                            if (refData.Name != null && CanAfford(refData, character))
                            {
                                StartQuest(refData, character, -1.5);
                            }
                        }
                    }
                }
            }
        }
    }
}
