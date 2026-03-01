using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerCoreTests
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
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.UsableLocations);
        Assert.AreEqual(6, _resourceManager.UsableLocations.Count);
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
    public void ResourceManager_CadenceAssignment_WorksCorrectly()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var character = _resourceManager!.Characters[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        var assigned = _resourceManager.JunctionManager.CurrentlyAssigned(character).ToList();
        
        Assert.AreEqual(1, assigned.Count);
        Assert.AreEqual(cadence.Name, assigned[0].Name);

        _resourceManager.JunctionManager.Unassign(cadence);
        assigned = _resourceManager.JunctionManager.CurrentlyAssigned(character).ToList();
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
    public void ResourceManager_UnlockCadence_UpdatesUnlockedList()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        
        _resourceManager!.UnlockCadence(cadence);
        
        Assert.IsTrue(_resourceManager.UnlockedCadences.Contains(cadence));
    }

    [TestMethod]
    public void ResourceManager_CanAfford_CadenceUnlock_ReturnsCorrectValue()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(unlock));

        foreach(var req in unlock.Requirements)
        {
            _resourceManager.Inventory.Add(req.Item, req.Quantity);
        }
        
        Assert.IsTrue(_resourceManager.CanAfford(unlock));
    }

    [TestMethod]
    public void LocationData_LockedQuests_ReturnsCorrectValues()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var initialQuestCount = village.Quests.Count;
        
        var allVillageQuests = ContentHost.GetContent<Locations>().All.First(l => l.Name == "Village").Quests;
        var locked = village.LockedQuests.ToList();
        
        Assert.AreEqual(allVillageQuests.Count() - initialQuestCount, locked.Count);
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_CadenceAbility_AddsToUnlocked()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];

        _resourceManager!.ReceiveRewards(unlock).Wait();
        
        Assert.IsTrue(_resourceManager.UnlockedAbilities.Contains(unlock.Ability));
    }

    [TestMethod]
    public void ResourceManager_StartQuest_CadenceUnlock_AddsToActiveQuests()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        
        _resourceManager.StartQuest(unlock, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.AreEqual(unlock.Ability.Name, _resourceManager.ActiveQuests[0].Name);
    }

    [TestMethod]
    public void ResourceManager_PayCosts_CadenceUnlock_RemovesItems()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];

        _resourceManager!.Inventory.Clear();
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        
        _resourceManager.PayCosts(unlock);
        
        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(unlock.Requirements[0].Item));
    }

    [TestMethod]
    public void Character_Name_ReturnsCorrectValue()
    {
        var character = new Character("Test");
        Assert.AreEqual("Test", character.Name);
    }
}
