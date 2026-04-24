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
        SandboxContent.Load();
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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(questData));

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        Assert.IsTrue(_resourceManager.CanAfford(questData));
    }

    [TestMethod]
    public void ResourceManager_CanAfford_WithCharacter_ChecksStats()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.ChopWood);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var himbo = _resourceManager!.Characters.First(c => c.Name == "Himbo");

        // Strength required: 10 in Sandbox. Base Himbo Strength: 10.
        Assert.IsTrue(_resourceManager.CanAfford(questData, himbo));

        // Let's test a higher requirement. 
        // We'll temporarily modify the quest detail for this test or use a different quest if we had one.
        // Actually, SandboxContent.ChopWood has Strength 10 requirement.
        // If we want to test failure, we need a character with < 10 strength or a quest with > 10.
        
        var weakling = new Character("Weakling"); // Assume base stats are low if not boosted? 
        // Actually, Characters in ResourceManager are initialized with base stats of 10.
        
        // Let's just boost the requirement for this specific test instance
        var highReqDetail = new QuestDetail(detail.DurationSeconds, detail.Requirements, detail.Rewards, detail.Type, detail.PrimaryStat, new System.Collections.Generic.Dictionary<string, int> { { SandboxContent.Strength, 20 } });
        var highReqQuestData = new QuestData(quest, highReqDetail);
        
        Assert.IsFalse(_resourceManager.CanAfford(highReqQuestData, himbo));

        _resourceManager.JunctionManager.AddStatBoost(himbo, SandboxContent.Strength, 10); // 10 + 10 = 20
        Assert.IsTrue(_resourceManager.CanAfford(highReqQuestData, himbo));
    }

    [TestMethod]
    public void ResourceManager_StartQuest_EnforcesTaskLimit()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 10000);
        
        // Default task limit is 1
        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should not exceed task limit.");
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RemovesFromActive()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        _resourceManager.StartQuest(questData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);

        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ResourceManager_StartQuest_PreventsDuplicateSingleUseTasks()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.Prologue);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should not start single-use quest twice.");
    }

    [TestMethod]
    public void ResourceManager_StartQuest_Refinement_Works()
    {
        var refinements = ContentHost.GetContent<ItemRefinements>();
        var ability = refinements.ByKey.Keys.First(a => a.Name == SandboxContent.RefineScrap);
        var inputItem = _items!.All.First(x => x.Name == SandboxContent.Scrap);
        var recipe = refinements.ByKey[ability].Recipes[inputItem];
        var refinementData = new RefinementData(ability, inputItem, recipe, SandboxContent.Strength);        

        var character = _resourceManager!.Characters[0];
        
        // Assign Recruit cadence to character so they have the ability
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.RefineScrap);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        _resourceManager.Inventory.Add(inputItem, 10);

        _resourceManager.StartQuest(refinementData, character);

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.AreEqual($"{SandboxContent.RefineScrap} ({SandboxContent.Scrap}): {SandboxContent.Gold}", _resourceManager.ActiveQuests[0].Name);      
    }
}
