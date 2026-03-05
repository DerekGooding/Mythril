namespace Mythril.Data;

public partial class ResourceManager
{
    public int GetTaskLimit(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        if (assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "Logistics II")))
            return 3;
        if (assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "Logistics I")))
            return 2;
        return 1;
    }

    public bool CanAutoQuest(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        return assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && (a.Ability.Name == "AutoQuest I" || a.Ability.Name == "AutoQuest II")));
    }

    public bool IsAutoQuestEnabled(Character character) => _autoQuestEnabled.TryGetValue(character.Name, out var enabled) && enabled;

    public void SetAutoQuestEnabled(Character character, bool enabled) => _autoQuestEnabled[character.Name] = enabled;

    public bool HasAutoQuestII(Character character)
    {
        var assigned = JunctionManager.CurrentlyAssigned(character);
        return assigned.Any(c => c.Abilities.Any(a => UnlockedAbilities.Contains($"{c.Name}:{a.Ability.Name}") && a.Ability.Name == "AutoQuest II"));
    }

    public async Task CompleteTaskAsync(QuestProgress completedProgress)
    {
        await ReceiveRewards(completedProgress);
        RemoveActiveQuest(completedProgress);

        // Auto-restart logic
        bool isRecurring = (completedProgress.Item is QuestData q && q.Type == QuestType.Recurring) || 
                          (completedProgress.Item is RefinementData);

        if (isRecurring && IsAutoQuestEnabled(completedProgress.Character) && CanAutoQuest(completedProgress.Character))
        {
            // Slot 0 restarts with AutoQuest I or II
            // Slot 1 restarts only with AutoQuest II
            bool shouldRestart = completedProgress.SlotIndex == 0 || (completedProgress.SlotIndex == 1 && HasAutoQuestII(completedProgress.Character));

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
