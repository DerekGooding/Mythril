using Mythril.Data.Items;

namespace Mythril.Data;

public class ResourceManager
{
    public int Gold => Inventory.GetQuantity("Gold");

    public List<Location> Locations { get; private set; } = [];
    public List<Character> Characters { get; private set; } = [];
    public List<Cadence> Cadences { get; private set; } = [];
    public InventoryManager? Inventory { get; private set; }
    public HashSet<string> CompletedTasks { get; } = [];

    public void SetData(List<Location> locations, List<Character> characters, List<Cadence> cadences, List<Item> items)
    {
        Locations = locations;
        Characters = characters;
        Cadences = cadences;
        Inventory = new InventoryManager(items);
        Inventory.Add("Potion"); // Starting Inventory
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

    public bool HasPrerequisites(Quest quest) => quest.Prerequisites.All(CompletedTasks.Contains);

    public void UpdateAvailableTasks()
    {
        
    }

    public void PayCosts(Quest task)
    {
        foreach (var requirement in task.Requirements)
            Inventory.Remove(requirement.Key, requirement.Value);
    }

    public void ReceiveRewards(Quest task)
    {
        foreach (var reward in task.Rewards)
            Inventory.Add(reward.Key, reward.Value);
        CompletedTasks.Add(task.Id ?? string.Empty);
        if (task.SingleUse)
        {
            foreach (var location in Locations)
            {
                location.Quests.Remove(task);
            }
        }
        UpdateAvailableTasks();
    }
}
