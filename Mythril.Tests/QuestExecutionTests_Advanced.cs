using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class QuestExecutionTests_Advanced
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private Quests? _quests;
    private QuestDetails? _questDetails;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var cadences = ContentHost.GetContent<Cadences>();
        var pathfinding = new PathfindingService(ContentHost.GetContent<Locations>(), _quests!, ContentHost.GetContent<QuestUnlocks>(), _questDetails!, cadences, ContentHost.GetContent<QuestToCadenceUnlocks>());
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), cadences);
        _resourceManager = new ResourceManager(gameStore, _items, _quests, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>(),
            pathfinding);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void ResourceManager_CanAfford_ChecksRequirements()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(questData));

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        Assert.IsTrue(_resourceManager.CanAfford(questData));
    }

    [TestMethod]
    public void ResourceManager_CanAfford_WithCharacter_ChecksStats()
    {
        var quest = _quests!.All.First(x => x.Name == "Defeat Treant Guardian");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Potion"), 10);
        
        // Ensure quest actually has the requirement in test data
        Assert.IsNotNull(questData.RequiredStats);
        Assert.IsTrue(questData.RequiredStats.ContainsKey("Strength"));
        Assert.AreEqual(25, questData.RequiredStats["Strength"]);

        // Strength required: 25. Base Himbo Strength: 10.
        Assert.IsFalse(_resourceManager.CanAfford(questData, himbo));

        _resourceManager.JunctionManager.AddStatBoost(himbo, "Strength", 15); // 10 + 15 = 25
        Assert.IsTrue(_resourceManager.CanAfford(questData, himbo));
    }

    [TestMethod]
    public void ResourceManager_StartQuest_EnforcesTaskLimit()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 10000);
        
        // Default task limit is 1
        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should not exceed task limit.");
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RemovesFromActive()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);

        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }
}

