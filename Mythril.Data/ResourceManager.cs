namespace Mythril.Data;

public class ResourceManager
{
    public int Gold => Inventory.GetQuantity("Gold");

    private Location[] _locations = [];
    private QuestUnlocks? _questUnlocks;
    public Character[] Characters { get; private set; } = [];
    public Cadence[] Cadences { get; private set; } = [];
    public InventoryManager Inventory { get; } = new InventoryManager();
    public HashSet<string> CompletedTasks { get; } = [];

    public IEnumerable<Location> UsableLocations = [];

    public void SetData(Character[] characters)
    {
        var items = ContentHost.GetContent<Items>();
        _locations = ContentHost.GetContent<Locations>().All;
        _questUnlocks = ContentHost.GetContent<QuestUnlocks>();
        Characters = characters;
        Cadences = ContentHost.GetContent<Cadences>().All;
        Inventory.Add(items.Potion.Name); // Starting Inventory
        Inventory.Add(items.Gold.Name, 100); // Starting Gold
        UpdateAvailableTasks();
    }

    public void AddGold(int amount) => Inventory.Add("Gold", amount);

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Inventory.Remove("Gold", amount);
            return true;
        }
        return false;
    }

    public bool CanAfford(Quest quest)
    {
        foreach (var requirement in quest.Requirements)
        {
            if (!Inventory.Has(requirement.Key, requirement.Value))
            {
                return false;
            }
        }

        return true;
    }

    public void UpdateAvailableTasks()
        => UsableLocations = _locations.Select(x => new Location(x.Name, [.. x.Quests.Where(Include)])).Where(l => l.Quests.Length > 0);

    private bool Include(Quest quest)
    {
        if(CompletedTasks.Contains(quest.Name) && quest.SingleUse)
            return false;
        if (_questUnlocks == null || _questUnlocks[quest].Length == 0)
            return true;

        return _questUnlocks[quest].All(r => CompletedTasks.Contains(r.Name));
    }

    public void PayCosts(Quest task)
    {
        foreach (var requirement in task.Requirements)
            Inventory.Remove(requirement.Key, requirement.Value);
    }

    public void ReceiveRewards(Quest quest)
    {
        foreach (var reward in quest.Rewards)
            Inventory.Add(reward.Key, reward.Value);
        CompletedTasks.Add(quest.Name);

        UpdateAvailableTasks();
    }
}
