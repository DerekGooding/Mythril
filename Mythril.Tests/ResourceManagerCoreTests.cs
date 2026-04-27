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
        SandboxContent.Load();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();

        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var cadences = ContentHost.GetContent<Cadences>();
        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests!,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails!,
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );
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
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        // Sandbox has 2 UsableLocations: Starting Area and Forest (initially locked)
        Assert.HasCount(1, _resourceManager!.UsableLocations);
        Assert.HasCount(3, _resourceManager.Characters);
    }

    [TestMethod]
    public void ResourceManager_RetrievesQuestData_Correctly()
    {
        // Assert
        var area = _resourceManager!.UsableLocations.First(l => l.Name == "Starting Area");
        var quest = area.Quests.FirstOrDefault(c => c.Name == SandboxContent.Prologue);
        Assert.AreNotEqual(default, quest);
        Assert.AreEqual(SandboxContent.Prologue, quest.Name);
    }

    [TestMethod]
    public void QuestData_Properties_ReturnCorrectValues()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.Prologue);
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
        var cadence = ContentHost.GetContent<Cadences>().All[0];
        var character = _resourceManager!.Characters[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        var assigned = _resourceManager.JunctionManager.CurrentlyAssigned(character).ToList();

        Assert.HasCount(1, assigned);
        Assert.AreEqual(cadence.Name, assigned[0].Name);

        _resourceManager.JunctionManager.Unassign(cadence, _resourceManager.UnlockedAbilities);
        assigned = [.. _resourceManager.JunctionManager.CurrentlyAssigned(character)];
        Assert.IsEmpty(assigned);
    }

    [TestMethod]
    public void Stats_All_ContainsAllStats()
    {
        var stats = ContentHost.GetContent<Stats>();
        Assert.HasCount(4, stats.All);
        Assert.Contains(s => s.Name == SandboxContent.Vitality, stats.All);
    }

    [TestMethod]
    public void ResourceManager_UnlockCadence_UpdatesUnlockedList()
    {
        var cadence = ContentHost.GetContent<Cadences>().All[0];

        _resourceManager!.UnlockCadence(cadence);

        Assert.Contains(cadence, _resourceManager.UnlockedCadences);
    }

    [TestMethod]
    public void ResourceManager_CanAfford_CadenceUnlock_ReturnsCorrectValue()
    {
        var cadence = ContentHost.GetContent<Cadences>().All[0];
        var unlock = cadence.Abilities[0];

        _resourceManager!.Inventory.Clear();
        // Sandbox abilities don't have item requirements, so CanAfford should be true if it has no requirements
        // Wait, let's re-read SandboxContent. None have requirements.
        Assert.IsTrue(_resourceManager.CanAfford(unlock));

        // Let's add a requirement for the sake of the test
        var reqUnlock = new CadenceUnlock(unlock.CadenceName, unlock.Ability, [new ItemQuantity(_items!.All.First(i => i.Name == SandboxContent.Gold), 100)], unlock.PrimaryStat);

        _resourceManager.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(reqUnlock));

        _resourceManager.Inventory.Add(_items.All.First(i => i.Name == SandboxContent.Gold), 100);
        Assert.IsTrue(_resourceManager.CanAfford(reqUnlock));
    }

    [TestMethod]
    public void LocationData_LockedQuests_ReturnsCorrectValues()
    {
        var area = _resourceManager!.UsableLocations.First(l => l.Name == "Starting Area");
        var initialQuestCount = area.Quests.Count;

        var allAreaQuests = ContentHost.GetContent<Locations>().All.First(l => l.Name == "Starting Area").Quests;
        var locked = area.LockedQuests.ToList();

        Assert.HasCount(allAreaQuests.Count() - initialQuestCount, locked);
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_CadenceAbility_AddsToUnlocked()
    {
        var cadence = ContentHost.GetContent<Cadences>().All[0];
        var unlock = cadence.Abilities[0];

        _resourceManager!.ReceiveRewards(unlock).Wait(TestContext.CancellationToken);

        Assert.Contains($"{unlock.CadenceName}:{unlock.Ability.Name}", _resourceManager.UnlockedAbilities);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_CadenceUnlock_AddsToActiveQuests()
    {
        var cadence = ContentHost.GetContent<Cadences>().All[0];
        var unlock = cadence.Abilities[0];
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        foreach (var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);

        _resourceManager.StartQuest(unlock, character);

        Assert.HasCount(1, _resourceManager.ActiveQuests);
        Assert.AreEqual(unlock.Ability.Name, _resourceManager.ActiveQuests[0].Name);
    }

    [TestMethod]
    public void ResourceManager_PayCosts_CadenceUnlock_RemovesItems()
    {
        var cadence = ContentHost.GetContent<Cadences>().All[0];
        var ability = cadence.Abilities[0].Ability;
        var unlock = new CadenceUnlock(cadence.Name, ability, [new ItemQuantity(_items!.All.First(i => i.Name == SandboxContent.Gold), 100)], "Magic");

        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(_items.All.First(i => i.Name == SandboxContent.Gold), 100);

        _resourceManager.PayCosts(unlock);

        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(unlock.Requirements[0].Item));
    }

    [TestMethod]
    public void ResourceManager_Initialize_ClearsJunctions()
    {
        // Setup a junction
        var character = _resourceManager!.Characters[0];
        var stat = ContentHost.GetContent<Stats>().All[0];
        var magic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        // Mock ability unlock
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.JStr);
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        _resourceManager.JunctionManager.JunctionMagic(character, stat, magic, _resourceManager.UnlockedAbilities);
        Assert.HasCount(1, _resourceManager.JunctionManager.Junctions);

        // Initialize
        _resourceManager.Initialize();

        // Assert
        Assert.IsEmpty(_resourceManager.JunctionManager.Junctions, "Junctions should be cleared on initialization.");
    }

    [TestMethod]
    public void Character_Name_ReturnsCorrectValue()
    {
        var character = new Character("Test");
        Assert.AreEqual("Test", character.Name);
    }

    public TestContext TestContext { get; set; }
}