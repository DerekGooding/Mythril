using Mythril.Data.Items;
using Mythril.Data.Jobs;

namespace Mythril.Data;

public class ResourceManager
{
    public int Gold => Inventory.GetQuantity("Gold");
    public int Mana => Inventory.GetQuantity("Mana");
    public int Faith => Inventory.GetQuantity("Faith");

    public List<Location> Locations { get; private set; } = [];
    public List<Character> Characters { get; private set; } = [];
    public List<Job> Jobs { get; private set; } = [];
    public List<Item> Items { get; private set; } = [];
    public InventoryManager Inventory { get; }
    public HashSet<string> CompletedTasks { get; } = [];

    public ResourceManager()
    {
        Inventory = new InventoryManager(this);
        Inventory.Add("Potion", 1); // Starting Inventory
    }

    public void SetData(List<Location> locations, List<Character> characters, List<Job> jobs, List<Item> items)
    {
        Locations = locations;
        Characters = characters;
        Jobs = jobs;
        Items = items;
        UpdateAvailableTasks();
    }

    public void AddGold(int amount) => Inventory.Add("Gold", amount);

    public void AddTask(string locationName, TaskData task)
    {
        var location = Locations.FirstOrDefault(l => l.Name == locationName);
        if (location != null)
        {
            location.Tasks.Add(task);
        }
    }

    public bool SpendGold(int amount) => Inventory.Remove("Gold", amount);

    public void AddMana(int amount) => Inventory.Add("Mana", amount);

    public void AddFaith(int amount) => Inventory.Add("Faith", amount);

    public bool UpgradeCharacterAttack(Character character)
    {
        var cost = character.AttackPower * 10;
        if (SpendGold(cost))
        {
            character.AttackPower++;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        // This will be more complex now, we need to clear the inventory.
        // For now, let's just reset the main resources.
        Inventory.Remove("Gold", Gold);
        Inventory.Remove("Mana", Mana);
        Inventory.Remove("Faith", Faith);
    }

    public bool CanAfford(TaskData task)
    {
        foreach (var requirement in task.Requirements)
        {
            if (!Inventory.Has(requirement.Key, requirement.Value))
            {
                return false;
            }
        }

        return true;
    }

    public bool HasPrerequisites(TaskData task) => task.Prerequisites.All(CompletedTasks.Contains);

    public void UpdateAvailableTasks()
    {
        foreach (var location in Locations)
        {
            location.Tasks = location.Tasks.Where(HasPrerequisites).ToList();
        }
    }

    public void PayCosts(TaskData task)
    {
        foreach (var requirement in task.Requirements)
            Inventory.Remove(requirement.Key, requirement.Value);
    }

    public void ReceiveRewards(TaskData task)
    {
        foreach (var reward in task.Rewards)
            Inventory.Add(reward.Key, reward.Value);
        CompletedTasks.Add(task.Id ?? string.Empty);
        if (task.SingleUse)
        {
            foreach (var location in Locations)
            {
                location.Tasks.Remove(task);
            }
        }
        UpdateAvailableTasks();
    }
}
