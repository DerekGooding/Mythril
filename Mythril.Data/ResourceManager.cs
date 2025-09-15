namespace Mythril.Data;

public class ResourceManager
{
    private readonly Items _items = ContentHost.GetContent<Items>();
    private readonly Location[] _locations = ContentHost.GetContent<Locations>().All;
    private readonly QuestUnlocks _questUnlocks = ContentHost.GetContent<QuestUnlocks>();

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];
    public readonly Cadence[] Cadences = ContentHost.GetContent<Cadences>().All;

    public Dictionary<Cadence, Character?> AssignedCadences = [];
    public InventoryManager Inventory { get; } = new InventoryManager();
    public HashSet<string> CompletedTasks { get; } = [];
    public HashSet<string> LockedTasks { get; } = [];

    public IEnumerable<Location> UsableLocations = [];

    public ResourceManager()
    {
        Inventory.Add(_items.Gold, 100);
        AssignedCadences = Cadences.ToNamedDictionary(_ => (Character?)null);
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

    public void ReceiveRewards(IEnumerable<ItemQuantity> rewards, string name)
    {
        foreach (var reward in rewards)
            Inventory.Add(reward.Item, reward.Quantity);
        CompletedTasks.Add(name);
        LockedTasks.Remove(name);

        UpdateAvailableTasks();
    }


    public void AssignCadence(Cadence cadence, Character character) => AssignedCadences[cadence] = character;
    public void Unassign(Cadence cadence) => AssignedCadences[cadence] = null;
    public IEnumerable<Cadence> CurrentlyAssigned(Character character)
        => AssignedCadences.Where(x => x.Value == character).Select(x => x.Key);
}
