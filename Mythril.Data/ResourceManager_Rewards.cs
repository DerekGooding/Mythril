namespace Mythril.Data;

public partial class ResourceManager
{
    public async Task ReceiveRewards(object item)
    {
        string taskName = "Unknown";
        string characterName = "Unknown";
        string details = "";

        if (item is QuestData quest)
        {
            taskName = quest.Name;
            details = "Completed Quest";
            foreach (var reward in quest.Rewards)
            {
                int overflow = Inventory.Add(reward.Item, reward.Quantity);
                if (overflow > 0) OnItemOverflow?.Invoke(reward.Item.Name, overflow);
            }
            
            // If it's single-use, remove it now that it's DONE
            if (quest.Type == QuestType.Single || quest.Type == QuestType.Unlock) LockQuest(quest.Quest);

            UnlockQuest(quest.Quest);
            foreach (var cadence in _questToCadenceUnlocks[quest.Quest]) UnlockCadence(cadence);
        }
        if(item is CadenceUnlock unlock)
        {
            taskName = unlock.Ability.Name;
            details = $"Unlocked {unlock.CadenceName} Ability";
            var alreadyDiscovered = UnlockedAbilities.Any(ua => ua.EndsWith($":{unlock.Ability.Name}"));
            UnlockedAbilities.Add($"{unlock.CadenceName}:{unlock.Ability.Name}");
            
            // Only set unseen if this ability actually unlocks something in the workshop
            // AND it wasn't already discovered via another cadence
            // AND the user isn't already looking at the workshop
            if (!alreadyDiscovered && _refinements.ByKey.ContainsKey(unlock.Ability) && ActiveTab != "workshop")
            {
                HasUnseenWorkshop = true;
            }
            UpdateMagicCapacity();
        }
        if(item is RefinementData refinement)
        {
            taskName = refinement.Name;
            details = $"Refined {refinement.Recipe.OutputQuantity}x {refinement.Recipe.OutputItem.Name}";
            int overflow = Inventory.Add(refinement.Recipe.OutputItem, refinement.Recipe.OutputQuantity);
            if (overflow > 0) OnItemOverflow?.Invoke(refinement.Recipe.OutputItem.Name, overflow);
        }

        // Add to journal
        lock(_questLock)
        {
            // Find character who was doing this
            var progress = ActiveQuests.FirstOrDefault(p => p.Item == item);
            if (progress != null) characterName = progress.Character.Name;
        }
        AddToJournal(taskName, characterName, details);

        await Task.CompletedTask;
    }

    public void RestoreCompletedQuest(Quest quest)
    {
        UnlockQuest(quest);
        foreach (var cadence in _questToCadenceUnlocks[quest])
        {
            UnlockCadence(cadence);
        }
    }
}
