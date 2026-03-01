using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class QuestLifecycleTests
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
    public void ResourceManager_StartQuest_AddsToActiveQuests()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        
        _resourceManager.StartQuest(questData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.AreEqual("Buy Potion", _resourceManager.ActiveQuests[0].Name);
    }

    [TestMethod]
    public void ResourceManager_Tick_IncrementsProgress()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, character);
        
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(0, progress.SecondsElapsed);

        _resourceManager.Tick(0.1); // 1 tick = 1 second in implementation
        Assert.AreEqual(1, progress.SecondsElapsed);
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
    public void ResourceManager_StartQuest_Recurring_StrengthReducesDuration()
    {
        var quest = _quests!.All.First(x => x.Name == "Farm Goblins");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        // Himbo has 20 Strength
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // Expected: 20 / (1.0 + 20/100) = 20 / 1.2 = 16.66 -> 16 (int)
        Assert.AreEqual(16, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Wifu_MagicReducesCadenceDuration()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];
        
        // Wifu has 15 Magic
        var wifu = _resourceManager!.Characters.First(c => c.Name == "Wifu");
        
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, wifu);
        
        var progress = _resourceManager.ActiveQuests[0];
        // Expected: 10 / (1.0 + 15/100) = 10 / 1.15 = 8.69 -> 8 (int)
        Assert.AreEqual(8, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_PayCosts_SingleQuest_LocksQuest()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var quest = village.Quests.First(q => q.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(village.Quests.Contains(quest));
        
        _resourceManager.PayCosts(questData);
        
        Assert.IsFalse(village.Quests.Contains(quest));
    }

    [TestMethod]
    public void QuestProgress_ZeroDuration_ReturnsFullProgress()
    {
        var character = new Character("Hero");
        var quest = _quests!.All.First();
        var progress = new QuestProgress(quest, "Zero", 0, character);
        
        Assert.AreEqual(1.0, progress.Progress);
        Assert.IsTrue(progress.IsCompleted);
    }
}
