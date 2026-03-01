using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
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
        
        _resourceManager = new ResourceManager(
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            ContentHost.GetContent<Cadences>(), 
            ContentHost.GetContent<Locations>());
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.UsableLocations);
        Assert.AreEqual(6, _resourceManager.UsableLocations.Count); // Increased by Content expansion
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.AreEqual(3, _resourceManager.Characters.Length);
    }

    [TestMethod]
    public void ResourceManager_RetrievesQuestData_Correctly()
    {
        // Assert
        var village = _resourceManager!.UsableLocations.FirstOrDefault(l => l.Name == "Village");
        Assert.IsNotNull(village);
        var quest = village.Quests.FirstOrDefault(c => c.Name == "Prologue");
        Assert.IsNotNull(quest);
        Assert.AreEqual("Prologue", quest.Name);
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
    public void QuestData_Properties_ReturnCorrectValues()
    {
        var quest = _quests!.All.First(x => x.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        Assert.AreEqual(quest.Name, questData.Name);
        Assert.AreEqual(quest.Description, questData.Description);
        Assert.AreEqual(detail.DurationSeconds, questData.DurationSeconds);
        Assert.AreEqual(detail.Type, questData.Type);
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
    public void ResourceManager_CadenceAssignment_WorksCorrectly()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var character = _resourceManager!.Characters[0];

        _resourceManager.AssignCadence(cadence, character);
        var assigned = _resourceManager.CurrentlyAssigned(character).ToList();
        
        Assert.AreEqual(1, assigned.Count);
        Assert.AreEqual(cadence.Name, assigned[0].Name);

        _resourceManager.Unassign(cadence);
        assigned = _resourceManager.CurrentlyAssigned(character).ToList();
        Assert.AreEqual(0, assigned.Count);
    }

    [TestMethod]
    public void Stats_All_ContainsAllStats()
    {
        var stats = ContentHost.GetContent<Stats>();
        Assert.AreEqual(9, stats.All.Length);
        Assert.IsTrue(stats.All.Any(s => s.Name == "Health"));
    }

    [TestMethod]
    public void Character_Name_ReturnsCorrectValue()
    {
        var character = new Character("Test");
        Assert.AreEqual("Test", character.Name);
    }
}
