namespace Mythril.Data;

public partial class ResourceManager
{
    public async Task ReceiveRewards(QuestProgress progress)
    {
        object item = progress.Item;
        string taskName = "Unknown";
        string characterName = progress.Character.Name;
        string details = "";

        if (item is QuestData questData)
        {
            taskName = questData.Name;
            details = "Completed " + questData.Name;
            
            // Add rewards
            if (questData.Rewards != null)
            {
                foreach (var reward in questData.Rewards)
                {
                    int overflow = Inventory.Add(reward.Item, reward.Quantity);
                    if (overflow > 0) OnItemOverflow?.Invoke(reward.Item.Name, overflow);
                }
            }

            // Stat rewards
            if (questData.StatRewards != null)
            {
                foreach (var statRew in questData.StatRewards)
                {
                    JunctionManager.AddStatBoost(progress.Character, statRew.Key, statRew.Value);
                }
            }

            // Unlocks
            if (questData.Type == QuestType.Single || questData.Type == QuestType.Unlock)
            {
                _gameStore.Dispatch(new CompleteQuestAction(questData.Quest));
                
                // Unlock Cadences
                var unlockedCadences = _questToCadenceUnlocks[questData.Quest];
                foreach (var cadence in unlockedCadences)
                {
                    UnlockCadence(cadence);
                }

                // Discover locations
                UpdateUsableLocations();
            }
        }
        else if (item is CadenceUnlock unlock)
        {
            taskName = unlock.Ability.Name;
            details = $"Researched {unlock.Ability.Name} for {unlock.CadenceName}";
            
            string abilityKey = $"{unlock.CadenceName}:{unlock.Ability.Name}";
            bool alreadyDiscovered = UnlockedAbilities.Contains(abilityKey);
            
            _gameStore.Dispatch(new UnlockAbilityAction(abilityKey));

            if (!alreadyDiscovered && _refinements.ByKey.ContainsKey(unlock.Ability) && ActiveTab != "workshop")
            {
                HasUnseenWorkshop = true;
            }
            UpdateMagicCapacity();
            JunctionManager.UpdatePassiveBoosts(progress.Character, UnlockedAbilities);
        }
        else if (item is RefinementData refinement)
        {
            taskName = refinement.Name;
            details = refinement.Description;
            
            int overflow = Inventory.Add(refinement.Recipe.OutputItem, refinement.Recipe.OutputQuantity);
            if (overflow > 0) OnItemOverflow?.Invoke(refinement.Recipe.OutputItem.Name, overflow);
        }

        // Journal Entry
        bool isFirstTime = !Journal.Any(j => j.TaskName == taskName);
        Journal.Insert(0, new JournalEntry(taskName, characterName, details, DateTime.Now, isFirstTime));
        
        _gameStore.Dispatch(new CancelQuestAction(progress));
    }

    public void UnlockCadence(Cadence cadence)
    {
        if (!UnlockedCadenceNames.Contains(cadence.Name))
        {
            _gameStore.Dispatch(new UnlockCadenceAction(cadence.Name));
            HasUnseenCadence = true;
        }
    }

    public async Task ReceiveRewards(object dummyProgress)
    {
        // This is for some legacy or generic calls, but usually we have a QuestProgress
        if (dummyProgress is QuestProgress p) await ReceiveRewards(p);
    }

    public void RestoreCompletedQuest(Quest quest)
    {
        _gameStore.Dispatch(new CompleteQuestAction(quest));
        foreach (var cadence in _questToCadenceUnlocks[quest])
        {
            UnlockCadence(cadence);
        }
    }
}
