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
        _resourceManager = new ResourceManager();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.UsableLocations);
        Assert.AreEqual(5, _resourceManager.UsableLocations.Count);
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
        var quest = _quests!.BuyPotion;
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(questData));

        _resourceManager.Inventory.Add(_items!.Gold, 1000);
        Assert.IsTrue(_resourceManager.CanAfford(questData));
    }

    [TestMethod]
    public void ResourceManager_PayCosts_RemovesItems()
    {
        var quest = _quests!.BuyPotion;
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.Gold, 1000);
        
        _resourceManager.PayCosts(questData);
        
        Assert.AreEqual(750, _resourceManager.Inventory.GetQuantity(_items.Gold));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_AddsItems()
    {
        var quest = _quests!.BuyPotion;
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        Assert.AreEqual(1, _resourceManager.Inventory.GetQuantity(_items!.Potion));
    }

    [TestMethod]
    public void QuestData_Properties_ReturnCorrectValues()
    {
        var quest = _quests!.Prologue;
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        Assert.AreEqual(quest.Name, questData.Name);
        Assert.AreEqual(quest.Description, questData.Description);
        Assert.AreEqual(detail.DurationSeconds, questData.DurationSeconds);
        Assert.AreEqual(detail.Type, questData.Type);
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
