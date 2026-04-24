using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;

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
        SandboxContent.Load();
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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        
        _resourceManager.StartQuest(questData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.AreEqual(SandboxContent.BuyPotion, _resourceManager.ActiveQuests[0].Name);
    }

    [TestMethod]
    public void ResourceManager_Tick_IncrementsProgress()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        
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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.ChopWood);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // ChopWood base is 20. Baseline stat is 10.
        Assert.AreEqual(20, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Recurring_Stat20ReducesDurationBy25Percent()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.ChopWood);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");
        _resourceManager.JunctionManager.AddStatBoost(himbo, SandboxContent.Strength, 10); // 10 + 10 = 20
        
        _resourceManager.StartQuest(questData, himbo);
        
        var progress = _resourceManager.ActiveQuests[0];
        // 20 * 0.75 = 15
        Assert.AreEqual(15, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Wifu_Stat10IsBaselineForCadence()
    {
        var cadence = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        var unlock = cadence.Abilities[0];
        
        var wifu = _resourceManager!.Characters.First(c => c.Name == "Wifu");
        
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, wifu);
        
        var progress = _resourceManager.ActiveQuests[0];
        // Base for cadences in current engine is 30, but let's check what it actually is in QuestData for CadenceUnlock
        // The duration comes from the character's base stats.
        Assert.AreEqual(30, progress.DurationSeconds);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_MultipleCadencesCanResearchSameAbility()
    {
        var cadences = ContentHost.GetContent<Cadences>();
        var recruit = cadences.All.First(c => c.Name == SandboxContent.Recruit);
        var apprentice = cadences.All.First(c => c.Name == SandboxContent.Apprentice);
        
        var recruitAutoQuest = recruit.Abilities.First(a => a.Ability.Name == SandboxContent.AutoQuestI);
        var apprenticeAutoQuest = apprentice.Abilities.First(a => a.Ability.Name == SandboxContent.AutoQuestI);
        
        var character1 = _resourceManager!.Characters[0];
        var character2 = _resourceManager!.Characters[1];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        
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
