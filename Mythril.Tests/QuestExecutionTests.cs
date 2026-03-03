using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class QuestExecutionTests
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

        _resourceManager.Tick(1.0); 
        Assert.AreEqual(1.0, progress.SecondsElapsed);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Recurring_StrengthReducesDuration()
    {
        var quest = _quests!.All.First(x => x.Name == "Farm Goblins");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(54, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Wifu_MagicReducesCadenceDuration()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];
        
        var wifu = _resourceManager!.Characters.First(c => c.Name == "Wifu");
        
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, wifu);
        
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(27, progress.DurationSeconds);
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
