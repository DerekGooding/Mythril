using Mythril.GameLogic.Combat;
using Mythril.GameLogic.Items;
using Mythril.GameLogic.Jobs;
using Mythril.GameLogic.Materia;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Mythril.GameLogic;

public class ResourceManager
{
    public int Gold { get; private set; }
    public int Mana { get; private set; }
    public int Faith { get; private set; }

    public List<CardData> Cards { get; private set; }
    public List<Character> Characters { get; private set; }
    public List<Materia.Materia> Materia { get; private set; }
    public List<Job> Jobs { get; private set; }
    public List<Item> Items { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public InventoryManager Inventory { get; private set; }

    public ResourceManager()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;

        Cards = new List<CardData>();
        Characters = new List<Character>();
        Materia = new List<Materia.Materia>();
        Jobs = new List<Job>();
        Items = new List<Item>();
        Enemies = new List<Enemy>();

        LoadData();

        Inventory = new InventoryManager(this);
    }

    private void LoadData()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new MateriaConverter(), new JobConverter(), new ItemConverter() }
        };

        var cardsData = JsonConvert.DeserializeObject<List<CardData>>(File.ReadAllText("Data/cards.json"));
        if (cardsData != null)
        {
            Cards = cardsData;
        }

        var charactersData = JsonConvert.DeserializeObject<List<Character>>(File.ReadAllText("Data/characters.json"));
        if (charactersData != null)
        {
            Characters = charactersData;
        }

        var materiaData = JsonConvert.DeserializeObject<List<Materia.Materia>>(File.ReadAllText("Data/materia.json"), settings);
        if (materiaData != null)
        {
            Materia = materiaData;
        }

        var jobsData = JsonConvert.DeserializeObject<List<Job>>(File.ReadAllText("Data/jobs.json"), settings);
        if (jobsData != null)
        {
            Jobs = jobsData;
        }

        var itemsData = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("Data/items.json"), settings);
        if (itemsData != null)
        {
            Items = itemsData;
        }

        var enemiesData = JsonConvert.DeserializeObject<List<Enemy>>(File.ReadAllText("Data/enemies.json"));
        if (enemiesData != null)
        {
            Enemies = enemiesData;
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        Game1.Log($"Gold: {Gold}");
    }

    public void AddMana(int amount)
    {
        Mana += amount;
        Game1.Log($"Mana: {Mana}");
    }

    public void AddFaith(int amount)
    {
        Faith += amount;
        Game1.Log($"Faith: {Faith}");
    }

    public void Reset()
    {
        Gold = 0;
        Mana = 0;
        Faith = 0;
    }
}
