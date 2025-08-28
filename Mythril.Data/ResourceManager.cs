namespace Mythril.Data;

public class ResourceManager
{
    public int Gold => Inventory.GetQuantity("Gold");

    public Location[] Locations { get; private set; } = [];
    public Character[] Characters { get; private set; } = [];
    public Cadence[] Cadences { get; private set; } = [];
    public InventoryManager Inventory { get; } = new InventoryManager();
    public HashSet<string> CompletedTasks { get; } = [];

    public void SetData(Character[] characters)
    {
        var items = ContentHost.GetContent<Items>();
        Locations = ContentHost.GetContent<Locations>().All;
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

    //public bool HasPrerequisites(Quest quest) => quest.Prerequisites.All(CompletedTasks.Contains);

    public void UpdateAvailableTasks()
    {
        //TODO        
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
        CompletedTasks.Add(quest.Name ?? string.Empty);
        if (quest.SingleUse)
        {
            foreach (var location in Locations)
            {
                location.Quests.Remove(quest);
            }
        }
        UpdateAvailableTasks();
    }
}
