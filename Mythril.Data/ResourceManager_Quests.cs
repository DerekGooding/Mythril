namespace Mythril.Data;

public partial class ResourceManager
{
    public bool IsNeverLocked(Quest quest) => _questUnlocks[quest].Length == 0;

    public bool CanAfford(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var requirement in quest.Requirements)
            {
                if (!Inventory.Has(requirement.Item, requirement.Quantity))
                {
                    return false;
                }
            }
        }

        if(item is CadenceUnlock ability)
        {
            foreach(var requirement in ability.Requirements)
            {
                if (!Inventory.Has(requirement.Item, requirement.Quantity))
                {
                    return false;
                }
            }
        }

        if(item is RefinementData refinement)
        {
            return Inventory.Has(refinement.InputItem, refinement.Recipe.InputQuantity);
        }

        return true;
    }

    public bool HasAbility(Character character, CadenceAbility ability)
    {
        return JunctionManager.CurrentlyAssigned(character).Any(cad => 
            cad.Abilities.Any(a => a.Ability.Name == ability.Name && UnlockedAbilities.Contains($"{cad.Name}:{a.Ability.Name}"))
        );
    }

    private void LockQuest(Quest quest)
    {
        foreach(var location in UsableLocations)
        {
            location.Quests.Remove(quest);
        }
    }

    private void UnlockQuest(Quest quest)
    {
        _completedQuests.Add(quest);
        foreach(var location in UsableLocations)
        {
            foreach(var data in location.LockedQuests)
            {
                if (_questDetails[data].Type == QuestType.Single && _completedQuests.Contains(data))
                    continue;
                if (IsComplete(_questUnlocks[data]))
                    location.Quests.Add(data);
            }
        }
    }

    private bool IsComplete(Quest[] quests) => quests.All(_completedQuests.Contains);

    public void PayCosts(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var requirement in quest.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
        if(item is CadenceUnlock unlock)
        {
            foreach (var requirement in unlock.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
        if(item is RefinementData refinement)
        {
            Inventory.Remove(refinement.InputItem, refinement.Recipe.InputQuantity);
        }
    }

    public void StartQuest(object item, Character character)
    {
        if (CanAfford(item))
        {
            // Safety check for single-use tasks already in progress
            if (item is CadenceUnlock || (item is QuestData q && (q.Type == QuestType.Single || q.Type == QuestType.Unlock)))
            {
                if (IsInProgress(item))
                {
                    Console.WriteLine($"Attempted to start single-use task '{item}' but it is already in progress.");
                    return;
                }
            }

            PayCosts(item);
            double duration = 10; // Default
            if (item is QuestData quest)
            {
                duration = IsTestMode ? 3 : quest.DurationSeconds;
                if (!IsTestMode)
                {
                    // Strength reduces recurring quest duration
                    if (quest.Type == QuestType.Recurring)
                    {
                        int strength = JunctionManager.GetStatValue(character, "Strength");
                        duration /= (1.0 + (strength / 100.0));
                    }
                    // Vitality reduces single quest duration
                    else if (quest.Type == QuestType.Single)
                    {
                        int vitality = JunctionManager.GetStatValue(character, "Vitality");
                        duration /= (1.0 + (vitality / 100.0));
                    }
                }
                lock(_questLock)
                {
                    ActiveQuests.Add(new QuestProgress(quest, quest.Description, (int)Math.Max(1, duration), character));
                }
            }
            if(item is CadenceUnlock unlock)
            {
                duration = IsTestMode ? 3 : 30; // Increased base duration for Cadence unlocks
                if (!IsTestMode)
                {
                    // Magic reduces cadence unlock duration
                    int magic = JunctionManager.GetStatValue(character, "Magic");
                    duration /= (1.0 + (magic / 100.0));
                }
                lock(_questLock)
                {
                    ActiveQuests.Add(new QuestProgress(unlock, unlock.Ability.Description, (int)Math.Max(1, duration), character));
                }
            }
            if(item is RefinementData refinement)
            {
                duration = IsTestMode ? 2 : 15; // Base duration for refinements
                if (!IsTestMode)
                {
                    // Strength reduces refinement duration (it's physical work)
                    int strength = JunctionManager.GetStatValue(character, "Strength");
                    duration /= (1.0 + (strength / 100.0));
                }
                lock(_questLock)
                {
                    ActiveQuests.Add(new QuestProgress(refinement, refinement.Description, (int)Math.Max(1, duration), character));
                }
            }
        }
    }

    public async Task ReceiveRewards(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var reward in quest.Rewards) Inventory.Add(reward.Item, reward.Quantity);
            
            // If it's single-use, remove it now that it's DONE
            if (quest.Type == QuestType.Single || quest.Type == QuestType.Unlock) LockQuest(quest.Quest);

            UnlockQuest(quest.Quest);
            foreach (var cadence in _questToCadenceUnlocks[quest.Quest]) UnlockCadence(cadence);
        }
        if(item is CadenceUnlock unlock)
        {
            var alreadyDiscovered = UnlockedAbilities.Any(ua => ua.EndsWith($":{unlock.Ability.Name}"));
            UnlockedAbilities.Add($"{unlock.CadenceName}:{unlock.Ability.Name}");
            
            // Only set unseen if this ability actually unlocks something in the workshop
            // AND it wasn't already discovered via another cadence
            if (!alreadyDiscovered && _refinements.ByKey.ContainsKey(unlock.Ability))
            {
                HasUnseenWorkshop = true;
            }
            UpdateMagicCapacity();
        }
        if(item is RefinementData refinement)
        {
            Inventory.Add(refinement.Recipe.OutputItem, refinement.Recipe.OutputQuantity);
        }
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
