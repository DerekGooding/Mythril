namespace Mythril.Data;

public class ResourceManager
{
    private readonly Items _items = ContentHost.GetContent<Items>();
    private readonly Location[] _locations = ContentHost.GetContent<Locations>().All;
    private readonly QuestUnlocks _questUnlocks = ContentHost.GetContent<QuestUnlocks>();

    public readonly Character[] Characters = [new Character("Protagonist"), new Character("Wifu"), new Character("Himbo")];
    public readonly Cadence[] Cadences = ContentHost.GetContent<Cadences>().All;
    public InventoryManager Inventory { get; } = new InventoryManager();
    public HashSet<string> CompletedTasks { get; } = [];

    public IEnumerable<Location> UsableLocations = [];

    public ResourceManager()
    {
        Inventory.Add(_items.Gold, 100);
        UpdateAvailableTasks();
    }

    public bool CanAfford(Quest quest)
    {
        foreach (var requirement in quest.Requirements)
        {
            if (!Inventory.Has(requirement.Item, requirement.Quantity))
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateAvailableTasks()
        => UsableLocations = _locations.Select(x => new Location(x.Name, x.Quests.Where(Include))).Where(l => l.Quests.Any());

    private bool Include(Quest quest)
        => (!CompletedTasks.Contains(quest.Name) || quest.Type != QuestType.Single)
            && (_questUnlocks == null || _questUnlocks[quest].Length == 0
            || _questUnlocks[quest].All(r => CompletedTasks.Contains(r.Name)));

    public void PayCosts(Quest task)
    {
        foreach (var requirement in task.Requirements)
            Inventory.Remove(requirement.Item, requirement.Quantity);
    }

    public void ReceiveRewards(Quest quest)
    {
        foreach (var reward in quest.Rewards)
            Inventory.Add(reward.Item, reward.Quantity);
        CompletedTasks.Add(quest.Name);

        UpdateAvailableTasks();
    }
}
