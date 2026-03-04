namespace Mythril.Data;

public partial class ResourceManager
{
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

    public bool CanAfford(object item, Character character)
    {
        if (!CanAfford(item)) return false;

        if (item is QuestData quest)
        {
            if (quest.RequiredStats != null)
            {
                foreach (var req in quest.RequiredStats)
                {
                    if (JunctionManager.GetStatValue(character, req.Key) < req.Value)
                    {
                        return false;
                    }
                }
            }
        }

        if (item is RefinementData refinement)
        {
            return HasAbility(character, refinement.Ability);
        }

        return true;
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
        UpdateUsableLocations();
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

    public void StartQuest(object item, Character character, double initialSecondsElapsed = 0)
    {
        if (CanAfford(item, character))
        {
            lock (_questLock)
            {
                // Task limit check
                var charQuests = ActiveQuests.Where(q => q.Character.Name == character.Name).ToList();
                if (charQuests.Count >= GetTaskLimit(character))
                {
                    Console.WriteLine($"Character {character.Name} is already at task limit.");
                    return;
                }

                // Safety check for single-use tasks already in progress
                if (item is CadenceUnlock || (item is QuestData q && (q.Type == QuestType.Single || q.Type == QuestType.Unlock)))
                {
                    if (IsInProgress(item))
                    {
                        Console.WriteLine($"Attempted to start single-use task '{item}' but it is already in progress.");
                        return;
                    }
                }
            }

            PayCosts(item);
            double duration = 10; // Default
            string primaryStat = "Vitality";

            // Find first available slot
            int slotIndex = 0;
            lock(_questLock)
            {
                var charQuests = ActiveQuests.Where(q => q.Character.Name == character.Name).ToList();
                for (int i = 0; i < GetTaskLimit(character); i++)
                {
                    if (!charQuests.Any(q => q.SlotIndex == i))
                    {
                        slotIndex = i;
                        break;
                    }
                }
            }

            if (item is QuestData quest)
            {
                duration = IsTestMode ? 3 : quest.DurationSeconds;
                primaryStat = quest.PrimaryStat;
                
                if (!IsTestMode)
                {
                    int statValue = JunctionManager.GetStatValue(character, primaryStat);
                    duration /= (1.0 + (statValue / 100.0));
                }

                duration = Math.Max(0.5, duration);

                lock(_questLock)
                {
                    var qp = new QuestProgress(quest, quest.Description, (int)Math.Max(1, duration), character, slotIndex);
                    qp.SecondsElapsed = initialSecondsElapsed;
                    ActiveQuests.Add(qp);
                }
            }
            if(item is CadenceUnlock unlock)
            {
                duration = IsTestMode ? 3 : 30; // Increased base duration for Cadence unlocks
                primaryStat = unlock.PrimaryStat;

                if (!IsTestMode)
                {
                    int statValue = JunctionManager.GetStatValue(character, primaryStat);
                    duration /= (1.0 + (statValue / 100.0));
                }

                duration = Math.Max(0.5, duration);

                lock(_questLock)
                {
                    var qp = new QuestProgress(unlock, unlock.Ability.Description, (int)Math.Max(1, duration), character, slotIndex);
                    qp.SecondsElapsed = initialSecondsElapsed;
                    ActiveQuests.Add(qp);
                }
            }
            if(item is RefinementData refinement)
            {
                duration = IsTestMode ? 2 : 15; // Base duration for refinements
                primaryStat = refinement.PrimaryStat;

                if (!IsTestMode)
                {
                    int statValue = JunctionManager.GetStatValue(character, primaryStat);
                    duration /= (1.0 + (statValue / 100.0));
                }

                duration = Math.Max(0.5, duration);

                lock(_questLock)
                {
                    var qp = new QuestProgress(refinement, refinement.Description, (int)Math.Max(1, duration), character, slotIndex);
                    qp.SecondsElapsed = initialSecondsElapsed;
                    ActiveQuests.Add(qp);
                }
            }
        }
    }

    public async Task ReceiveRewards(object item)
    {
        if (item is QuestData quest)
        {
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
            int overflow = Inventory.Add(refinement.Recipe.OutputItem, refinement.Recipe.OutputQuantity);
            if (overflow > 0) OnItemOverflow?.Invoke(refinement.Recipe.OutputItem.Name, overflow);
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
