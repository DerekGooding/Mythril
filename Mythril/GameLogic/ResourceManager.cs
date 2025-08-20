using Mythril.Data;
using Mythril.Data.Items;
using Mythril.Data.Jobs;

namespace Mythril.GameLogic;

public class ResourceManager
{
    public int Gold { get; private set; }
    public int Mana { get; private set; }
    public int Faith { get; private set; }

    public List<CardData> Cards { get; private set; }
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

        Cards = [];
        Characters = [];
        Materia = [];
        Jobs = [];
        Items = [];
        Enemies = [];

        Inventory = new InventoryManager(this);
    }

    public void SetData(List<CardData> cards, List<Character> characters, List<Mythril.Data.Materia.Materia> materia, List<Job> jobs, List<Item> items, List<Enemy> enemies)
    {
        Cards = cards;
        Characters = characters;
        Materia = materia;
        Jobs = jobs;
        Items = items;
        Enemies = enemies;
    }

    public void AddGold(int amount) => Gold += amount;

    public void AddMana(int amount) => Mana += amount;

    public void AddFaith(int amount) => Faith += amount;

    public void Reset()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;
    }
}
