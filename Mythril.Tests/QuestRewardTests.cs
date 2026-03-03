using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class QuestRewardTests
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
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), ContentHost.GetContent<Cadences>());
        _resourceManager = new ResourceManager(
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            ContentHost.GetContent<Cadences>(), 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void ResourceManager_CanAfford_ReturnsCorrectValue()
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
    public void ResourceManager_PayCosts_RemovesItems()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        
        _resourceManager.PayCosts(questData);
        
        Assert.AreEqual(750, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == "Gold")));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_AddsItems()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        Assert.AreEqual(1, _resourceManager.Inventory.GetQuantity(_items!.All.First(x => x.Name == "Potion")));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_Quest_UnlocksCadence()
    {
        var quest = _quests!.All.First(x => x.Name == "Ancient Inscriptions");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        _resourceManager!.ReceiveRewards(questData).Wait();
    }

    [TestMethod]
    public void ResourceManager_PayCosts_SingleQuest_DoesNotLockQuest()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var quest = village.Quests.First(q => q.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(village.Quests.Contains(quest));
        
        _resourceManager.PayCosts(questData);
        
        Assert.IsTrue(village.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_SingleQuest_LocksQuest()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var quest = village.Quests.First(q => q.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(village.Quests.Contains(quest));
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        Assert.IsFalse(village.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RefundsCosts()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == "Gold");

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);
        
        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(750, _resourceManager.Inventory.GetQuantity(gold));

        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);

        Assert.AreEqual(1000, _resourceManager.Inventory.GetQuantity(gold));
    }

    [TestMethod]
    public void ResourceManager_Initialize_ClearsState()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, character);
        _resourceManager.ReceiveRewards(questData).Wait();

        Assert.IsTrue(_resourceManager.GetCompletedQuests().Any());
        
        _resourceManager.Initialize();

        Assert.IsFalse(_resourceManager.ActiveQuests.Any());
        Assert.IsFalse(_resourceManager.GetCompletedQuests().Any());
        Assert.AreEqual(100, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == "Gold")));
    }
}
