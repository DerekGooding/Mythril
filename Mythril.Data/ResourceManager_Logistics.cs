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

    public int GetAutoQuestLevel(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        int maxAuto = 0;
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

    public bool CanAutoQuest(Character character) => GetAutoQuestLevel(character) > 0;

    public bool IsAutoQuestEnabled(Character character) => _autoQuestEnabled.TryGetValue(character.Name, out var enabled) && enabled;

    public void SetAutoQuestEnabled(Character character, bool enabled) => _autoQuestEnabled[character.Name] = enabled;

    public async Task CompleteTaskAsync(QuestProgress completedProgress)
    {
        await ReceiveRewards(completedProgress);
        RemoveActiveQuest(completedProgress);

        // Auto-restart logic
        bool isRecurring = (completedProgress.Item is QuestData q && q.Type == QuestType.Recurring) || 
                          (completedProgress.Item is RefinementData);

        if (isRecurring && IsAutoQuestEnabled(completedProgress.Character))
        {
            int autoLevel = GetAutoQuestLevel(completedProgress.Character);
            bool shouldRestart = completedProgress.SlotIndex < autoLevel;

            if (shouldRestart && completedProgress.Item is RefinementData refinement)
            {
                if (refinement.Recipe.OutputItem.ItemType == ItemType.Spell && 
                    Inventory.GetQuantity(refinement.Recipe.OutputItem) >= Inventory.MagicCapacity)
                {
                    shouldRestart = false;
                }
            }

            if (shouldRestart)
            {
                if (CanAfford(completedProgress.Item, completedProgress.Character))
                {
                    // Use -1.5 as initialSecondsElapsed to provide a "preparing" visual delay in the UI
                    StartQuest(completedProgress.Item, completedProgress.Character, -1.5);
                }
            }
        }
    }
}
