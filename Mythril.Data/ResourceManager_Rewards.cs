namespace Mythril.Data;

public partial class ResourceManager
{
    public async Task ReceiveRewards(QuestProgress progress)
    {
        var alreadyDiscovered = false;
        if (progress.Item is CadenceUnlock unlock)
        {
            alreadyDiscovered = UnlockedAbilities.Contains($"{unlock.CadenceName}:{unlock.Ability.Name}");
        }

        _gameStore.Dispatch(new FinishQuestAction(progress));

        // Handle side effects that are still in manager for now
        if (progress.Item is QuestData questData && (questData.Type == QuestType.Single || questData.Type == QuestType.Unlock))
        {
            UpdateUsableLocations();
        }
        else if (progress.Item is CadenceUnlock unlock2)
        {
            if (!alreadyDiscovered && _refinements.ByKey.ContainsKey(unlock2.Ability) && ActiveTab != "workshop")
            {
                HasUnseenWorkshop = true;
            }
        }

        CheckAutoQuestTick();
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
        if (dummyProgress is QuestProgress p) await ReceiveRewards(p);
        else if (dummyProgress is QuestData qd) await ReceiveRewards(new QuestProgress(qd, qd.Description, 0, new Character("Unknown"), 0));
        else if (dummyProgress is CadenceUnlock cu) await ReceiveRewards(new QuestProgress(cu, cu.Ability.Description, 0, new Character("Unknown"), 0));
        else if (dummyProgress is RefinementData rd) await ReceiveRewards(new QuestProgress(rd, rd.Description, 0, new Character("Unknown"), 0));
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