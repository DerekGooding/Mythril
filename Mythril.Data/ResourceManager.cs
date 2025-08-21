using Mythril.Data.Items;
using Mythril.Data.Jobs;

namespace Mythril.Data;

public class ResourceManager
{
    public int Gold { get; private set; }
    public int Mana { get; private set; }
    public int Faith { get; private set; }

    public List<TaskData> Tasks { get; private set; }
    public List<Character> Characters { get; private set; }
    public List<Mythril.Data.Materia.Materia> Materia { get; private set; }
    public List<Job> Jobs { get; private set; }
    public List<Item> Items { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public InventoryManager Inventory { get; }

    public ResourceManager()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;

        Tasks = [];
        Characters = [];
        Materia = [];
        Jobs = [];
        Items = [];
        Enemies = [];

        Inventory = new InventoryManager(this);
    }

    public void SetData(List<TaskData> tasks, List<Character> characters, List<Mythril.Data.Materia.Materia> materia, List<Job> jobs, List<Item> items, List<Enemy> enemies)
    {
        Tasks = tasks;
        Characters = characters;
        Materia = materia;
        Jobs = jobs;
        Items = items;
        Enemies = enemies;
    }

    public void AddGold(int amount) => Gold += amount;

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public void AddMana(int amount) => Mana += amount;

    public void AddFaith(int amount) => Faith += amount;

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
        Gold = 0;
        Mana = 0;
        Faith = 0;
    }
}
