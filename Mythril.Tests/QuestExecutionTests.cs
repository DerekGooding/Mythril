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
        
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

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
        progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(1.0, progress.SecondsElapsed);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_WithNegativeDelay_TicksToZero()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        
        // Start with -1.5s delay
        _resourceManager.StartQuest(questData, character, -1.5);
        
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(-1.5, progress.SecondsElapsed);
        Assert.AreEqual(0, progress.Progress, "Progress should be 0 during delay.");

        _resourceManager.Tick(1.0); 
        progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(-0.5, progress.SecondsElapsed);
        Assert.AreEqual(0, progress.Progress, "Progress should still be 0 during delay.");

        _resourceManager.Tick(1.0); 
        progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(0.5, progress.SecondsElapsed);
        Assert.IsTrue(progress.Progress > 0, "Progress should be positive after delay.");
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Recurring_Stat10IsBaseline()
    {
        var quest = _quests!.All.First(x => x.Name == "Hunt Goblins");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // Base is 60. Stat 10 is baseline, so no reduction.
        Assert.AreEqual(60, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Recurring_Stat20ReducesDurationBy25Percent()
    {
        var quest = _quests!.All.First(x => x.Name == "Hunt Goblins");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        _resourceManager.JunctionManager.AddStatBoost(himbo, "Strength", 10); // 10 + 10 = 20
        
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // 60 * 0.75 = 45
        Assert.AreEqual(45, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Recurring_Stat30ReducesDurationByAlmost50Percent()
    {
        var quest = _quests!.All.First(x => x.Name == "Hunt Goblins");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        _resourceManager.JunctionManager.AddStatBoost(himbo, "Strength", 20); // 10 + 20 = 30
        
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // 60 * 0.5625 = 33.75 -> 33
        Assert.AreEqual(33, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Wifu_Stat10IsBaselineForCadence()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];
        
        var wifu = _resourceManager!.Characters.First(c => c.Name == "Wifu");
        
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, wifu);
        
        var progress = _resourceManager.ActiveQuests[0];
        // Base is 30. Stat 10 is baseline.
        Assert.AreEqual(30, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_MultipleCadencesCanResearchSameAbility()
    {
        var cadences = ContentHost.GetContent<Cadences>();
        var recruit = cadences.All.First(c => c.Name == "Recruit");
        var apprentice = cadences.All.First(c => c.Name == "Apprentice");
        
        var recruitAutoQuest = recruit.Abilities.First(a => a.Ability.Name == "AutoQuest I");
        var apprenticeAutoQuest = apprentice.Abilities.First(a => a.Ability.Name == "AutoQuest I");
        
        var character1 = _resourceManager!.Characters[0];
        var character2 = _resourceManager!.Characters[1];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        
        // Start Recruit research
        _resourceManager.StartQuest(recruitAutoQuest, character1);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        
        // Try starting Recruit research AGAIN (should fail)
        _resourceManager.StartQuest(recruitAutoQuest, character2);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should NOT start the same cadence's research twice.");
        
        // Start Apprentice research (should succeed)
        _resourceManager.StartQuest(apprenticeAutoQuest, character2);
        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count, "Should allow different cadences to research same ability name.");
    }
}
