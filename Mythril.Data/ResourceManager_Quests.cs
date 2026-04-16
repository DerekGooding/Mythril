using System.Collections.Generic;
using System.Linq;

namespace Mythril.Data;

public partial class ResourceManager
{
    public void StartQuest(object item, Character character, double initialSecondsElapsed = 0)
    {
        lock (_questLock)
        {
            if (ActiveQuests.Count(p => p.Character.Name == character.Name) >= GetTaskLimit(character)) return;

            // Single tasks can only be active once
            if (item is QuestData questData && (questData.Type == QuestType.Single || questData.Type == QuestType.Unlock))
            {
                if (IsInProgress(questData) || _gameStore.State.CompletedQuests.Contains(questData.Name)) return;
            }
            else if (item is CadenceUnlock unlock)
            {
                if (IsInProgress(unlock) || _gameStore.State.UnlockedAbilities.Contains($"{unlock.CadenceName}:{unlock.Ability.Name}")) return;
            }

            if (!CanAfford(item, character)) return;

            PayCosts(item);

            var description = item is QuestData qd ? qd.Description : (item is CadenceUnlock cu ? cu.Ability.Description : (item is RefinementData rd ? rd.Description : ""));
            int baseDuration = item is QuestData q ? q.DurationSeconds : (item is CadenceUnlock u ? 30 : (item is RefinementData r ? 15 : 10));
            
            // Apply stat-based duration scaling
            string primaryStat = item is QuestData q2 ? q2.PrimaryStat : (item is CadenceUnlock u2 ? u2.PrimaryStat : (item is RefinementData r2 ? r2.PrimaryStat : "Vitality"));
            double statValue = JunctionManager.GetStatValue(character, primaryStat);
            int duration = (int)(baseDuration * Math.Pow(0.75, (statValue - 10) / 10.0));

            // Find free slot
            var usedSlots = ActiveQuests.Where(p => p.Character.Name == character.Name).Select(p => p.SlotIndex).ToHashSet();
            int slot = 0;
            while (usedSlots.Contains(slot)) slot++;

            var progress = new QuestProgress(item, description, duration, character, slot) { SecondsElapsed = initialSecondsElapsed };
            _gameStore.Dispatch(new StartQuestAction(progress));
        }
    }

    public void CancelQuest(QuestProgress progress)
    {
        lock (_questLock)
        {
            _gameStore.Dispatch(new CancelQuestAction(progress));
            RefundCosts(progress.Item);
        }
    }

    public bool CanAfford(object item, Character? character = null)
    {
        if (item is Cadence) return true;
        if (item is QuestData quest)
        {
            if (quest.Requirements != null)
            {
                foreach (var req in quest.Requirements)
                {
                    if (Inventory.GetQuantity(req.Item) < req.Quantity) return false;
                }
            }
            if (character != null && quest.RequiredStats != null)
            {
                foreach (var stat in quest.RequiredStats)
                {
                    if (JunctionManager.GetStatValue(character.Value, stat.Key) < stat.Value) return false;
                }
            }
            return true;
        }
        else if (item is CadenceUnlock unlock)
        {
            if (unlock.Requirements != null)
            {
                foreach (var req in unlock.Requirements)
                {
                    if (Inventory.GetQuantity(req.Item) < req.Quantity) return false;
                }
            }
            return true;
        }
        else if (item is RefinementData refinement)
        {
            if (!Inventory.Has(refinement.InputItem, refinement.Recipe.InputQuantity)) return false;
            
            // Refinements require the ability to be UNLOCKED and the cadence ASSIGNED
            if (character == null) return false;
            return HasAbility(character.Value, refinement.Ability);
        }
        return false;
    }

    public void PayCosts(object item)
    {
        if (item is QuestData quest && quest.Requirements != null)
        {
            foreach (var req in quest.Requirements) Inventory.Remove(req.Item, req.Quantity);
        }
        else if (item is CadenceUnlock unlock && unlock.Requirements != null)
        {
            foreach (var req in unlock.Requirements) Inventory.Remove(req.Item, req.Quantity);
        }
        else if (item is RefinementData refinement)
        {
            Inventory.Remove(refinement.InputItem, refinement.Recipe.InputQuantity);
        }
    }

    private void RefundCosts(object item)
    {
        if (item is QuestData quest && quest.Requirements != null)
        {
            foreach (var req in quest.Requirements) Inventory.Add(req.Item, req.Quantity);
        }
        else if (item is CadenceUnlock unlock && unlock.Requirements != null)
        {
            foreach (var req in unlock.Requirements) Inventory.Add(req.Item, req.Quantity);
        }
        else if (item is RefinementData refinement)
        {
            Inventory.Add(refinement.InputItem, refinement.Recipe.InputQuantity);
        }
    }

    public bool IsInProgress(object item)
    {
        if (item is QuestData q) return ActiveQuests.Any(p => p.Item is QuestData qd && qd.Name == q.Name);
        if (item is CadenceUnlock u) return ActiveQuests.Any(p => p.Item is CadenceUnlock ud && ud.Ability.Name == u.Ability.Name && ud.CadenceName == u.CadenceName);
        return false;
    }
}
