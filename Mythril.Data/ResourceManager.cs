namespace Mythril.Data;

public class ResourceManager
{
    private readonly Items _items = ContentHost.GetContent<Items>();
    private readonly QuestUnlocks _questUnlocks = ContentHost.GetContent<QuestUnlocks>();
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();
    private readonly QuestDetails _questDetails = ContentHost.GetContent<QuestDetails>();

    public QuestDetail GetQuestDetails(Quest quest) => _questDetails[quest];

    private readonly Dictionary<Cadence, Character?> _assignedCadences;
    private readonly Dictionary<Cadence, bool> _lockedCadences;

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];
    private readonly Cadence[] _cadences = ContentHost.GetContent<Cadences>().All;

    public InventoryManager Inventory { get; } = new InventoryManager();

    private readonly HashSet<Quest> _completedQuests = [];

    public List<LocationData> UsableLocations;

    public List<Cadence> UnlockedCadences = [];


    public ResourceManager()
    {
        Inventory.Add(_items.Gold, 100);
        _assignedCadences = _cadences.ToNamedDictionary(_ => (Character?)null);
        _lockedCadences = _cadences.ToNamedDictionary(_ => true);

        UsableLocations = [.. ContentHost.GetContent<Locations>().All.Select(x => new LocationData(x, x.Quests.Where(IsNeverLocked)))];
    }

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

        return true;
    }

    public void UpdateAvaiableCadences()
        => UnlockedCadences = [.. _lockedCadences.Where(x => !x.Value).Select(x => x.Key)];

    //private bool Include(Quest quest)
    //    => (!CompletedTasks.Contains(quest.Name) || quest.Type != QuestType.Single)
    //        && !LockedTasks.Contains(quest.Name)
    //        && (_questUnlocks == null || _questUnlocks[quest].Length == 0
    //        || _questUnlocks[quest].All(r => CompletedTasks.Contains(r.Name)));

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
                if (_questDetails[quest].Type == QuestType.Single && _completedQuests.Contains(data))
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
            if (quest.Type == QuestType.Single)
            {
                LockQuest(quest.Quest);
            }

            foreach (var requirement in quest.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
        if(item is CadenceUnlock unlock)
        {
            foreach (var requirement in unlock.Requirements)
                Inventory.Remove(requirement.Item, requirement.Quantity);
        }
    }

    public Task ReceiveRewards(object item)
    {
        if (item is QuestData quest)
        {
            foreach (var reward in quest.Rewards)
                Inventory.Add(reward.Item, reward.Quantity);

            UnlockQuest(quest.Quest);

            foreach (var cadence in _questToCadenceUnlocks[quest.Quest])
                UnlockCadence(cadence);


        }
        if(item is CadenceUnlock unlock)
        {
            //TODO : Handle CadenceUnlock
            _ = unlock;
        }
        return Task.CompletedTask;
    }

    public void UnlockCadence(Cadence cadence)
    {
        _lockedCadences[cadence] = false;
        UpdateAvaiableCadences();
    }

    public void AssignCadence(Cadence cadence, Character character) => _assignedCadences[cadence] = character;
    public void Unassign(Cadence cadence) => _assignedCadences[cadence] = null;
    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => _assignedCadences.Where(x => x.Value == character).Select(x => x.Key);
}
