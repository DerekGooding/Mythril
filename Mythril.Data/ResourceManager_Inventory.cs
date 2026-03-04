namespace Mythril.Data;

public partial class ResourceManager
{
    public void RemoveActiveQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            ActiveQuests.Remove(progress);
        }
    }

    public void CancelQuest(QuestProgress progress)
    {
        lock(_questLock)
        {
            if (ActiveQuests.Contains(progress))
            {
                RefundCosts(progress.Item);
                ActiveQuests.Remove(progress);
            }
        }
    }

    private void RefundCosts(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var requirement in quest.Requirements)
                Inventory.Add(requirement.Item, requirement.Quantity);
        }
        if (item is CadenceUnlock unlock)
        {
            foreach (var requirement in unlock.Requirements)
                Inventory.Add(requirement.Item, requirement.Quantity);
        }
        if (item is RefinementData refinement)
        {
            Inventory.Add(refinement.InputItem, refinement.Recipe.InputQuantity);
        }
    }

    public bool IsInProgress(object item)
    {
        lock(_questLock)
        {
            if (item is QuestData quest)
            {
                return ActiveQuests.Any(p => p.Item is QuestData activeQuest && activeQuest.Quest.Name == quest.Quest.Name);
            }
            if (item is CadenceUnlock unlock)
            {
                return ActiveQuests.Any(p => p.Item is CadenceUnlock activeUnlock && activeUnlock.CadenceName == unlock.CadenceName && activeUnlock.Ability.Name == unlock.Ability.Name);
            }
            if (item is RefinementData refinement)
            {
                return ActiveQuests.Any(p => p.Item is RefinementData activeRefinement && 
                    activeRefinement.Ability.Name == refinement.Ability.Name && 
                    activeRefinement.InputItem.Name == refinement.InputItem.Name);
            }
            return false;
        }
    }

    public void CancelExcessQuests(Character character)
    {
        int limit = GetTaskLimit(character);
        lock (_questLock)
        {
            var charQuests = ActiveQuests.Where(q => q.Character.Name == character.Name).ToList();
            while (charQuests.Count > limit)
            {
                var toCancel = charQuests.Last();
                charQuests.Remove(toCancel);
                CancelQuest(toCancel);
            }
        }
    }
}
