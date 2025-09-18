namespace Mythril.Data;

public class ResourceManager
{
    private readonly Items _items = ContentHost.GetContent<Items>();
    private readonly Location[] _locations = ContentHost.GetContent<Locations>().All;
    private readonly QuestUnlocks _questUnlocks = ContentHost.GetContent<QuestUnlocks>();
    private readonly QuestToCadenceUnlocks _questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();

    private readonly Dictionary<Cadence, Character?> _assignedCadences;
    private readonly Dictionary<Cadence, bool> _lockedCadences;

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];
    private readonly Cadence[] _cadences = ContentHost.GetContent<Cadences>().All;

    public InventoryManager Inventory { get; } = new InventoryManager();
    public HashSet<string> CompletedTasks { get; } = [];
    public HashSet<string> LockedTasks { get; } = [];

    public IEnumerable<Location> UsableLocations = [];

    public IEnumerable<Cadence> UnlockedCadences = [];


    public ResourceManager()
    {
        Inventory.Add(_items.Gold, 100);
        _assignedCadences = _cadences.ToNamedDictionary(_ => (Character?)null);
        _lockedCadences = _cadences.ToNamedDictionary(_ => true);
        UpdateAvailableTasks();
    }

    public bool CanAfford(object item)
    {
        if (item is Quest quest)
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

    public void UpdateAvailableTasks()
        => UsableLocations = _locations.Select(x => new Location(x.Name, x.Quests.Where(Include))).Where(l => l.Quests.Any());
    public void UpdateAvaiableCadences()
        => UnlockedCadences = _lockedCadences.Where(x => !x.Value).Select(x => x.Key);

    private bool Include(Quest quest)
        => (!CompletedTasks.Contains(quest.Name) || quest.Type != QuestType.Single)
            && !LockedTasks.Contains(quest.Name)
            && (_questUnlocks == null || _questUnlocks[quest].Length == 0
            || _questUnlocks[quest].All(r => CompletedTasks.Contains(r.Name)));

    public void PayCosts(object item)
    {
        if (item is Quest quest)
        {
            if (quest.Type == QuestType.Single)
                LockedTasks.Add(quest.Name);

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
        if (item is Quest quest)
        {
            foreach (var reward in quest.Rewards)
                Inventory.Add(reward.Item, reward.Quantity);

            CompletedTasks.Add(quest.Name);
            LockedTasks.Remove(quest.Name);

            foreach (var cadence in _questToCadenceUnlocks[quest])
                UnlockCadence(cadence);

            UpdateAvailableTasks();
        }
        if(item is CadenceUnlock unlock)
        {
            //TODO : Handle CadenceUnlock
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
