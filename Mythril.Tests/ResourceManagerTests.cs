using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
{
    private ResourceManager? _resourceManager;
    private Quests? _quests;
    private QuestDetails? _questDetails;
    private Cadences? _cadences;
    private GameStore? _gameStore;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        var items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        _gameStore = new GameStore();
        var inventory = new InventoryManager(_gameStore);
        var junctionManager = new JunctionManager(_gameStore, inventory, ContentHost.GetContent<StatAugments>(), _cadences);
        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests!,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails!,
            _cadences!,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );
        _resourceManager = new ResourceManager(
            _gameStore,
            items, 
            _quests!,
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>(),
            pathfinding);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void ResourceManager_ReevaluateActiveQuests_Works()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility("Scholar", "Logistics II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        
        // Initial limit is 1. With Logistics II, it should be 3.
        Assert.AreEqual(3, _resourceManager.GetTaskLimit(character));
        
        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == "Hunt Bats"), _questDetails[_quests.All.First(q => q.Name == "Hunt Bats")]);
        var q3 = new QuestData(_quests.All.First(q => q.Name == "Hunt Spiders"), _questDetails[_quests.All.First(q => q.Name == "Hunt Spiders")]);
        
        _resourceManager.StartQuest(q1, character);
        _resourceManager.StartQuest(q2, character);
        _resourceManager.StartQuest(q3, character);
        
        Assert.AreEqual(3, _resourceManager.ActiveQuests.Count);
        
        // Remove assignment
        _resourceManager.JunctionManager.Unassign(scholar, _resourceManager.UnlockedAbilities);
        
        // Manual call or via event
        _resourceManager.ReevaluateActiveQuests(character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsOnRequirementFailure()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility("Recruit", "J-Str");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        // Add 100 Magic I to inventory for Strength junction
        var magicI = new Item("Magic I", "Magic", ItemType.Spell);
        _resourceManager.Inventory.Add(magicI, 100);
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat("Strength", ""), magicI, _resourceManager.UnlockedAbilities);

        // Verify strength > 10 (base 10 + 100/10 = 20)
        Assert.IsTrue(_resourceManager.JunctionManager.GetStatValue(character, "Strength") >= 20);

        // Create quest with Strength 15 requirement
        var quest = new Quest("Str Quest", "Requires 15 Str");
        var detail = new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { "Strength", 15 } });
        var questData = new QuestData(quest, detail);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        // Remove magic junction -> strength falls to 10
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat("Strength", ""), new Item(), _resourceManager.UnlockedAbilities);
        Assert.AreEqual(10, _resourceManager.JunctionManager.GetStatValue(character, "Strength"));

        // Reevaluate should cancel the quest
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsRefinementOnAbilityLoss()
    {
        var character = _resourceManager!.Characters[0];
        var refData = _resourceManager.Refinements.GetRefinement("Refine Fire", "Basic Gem")!.Value;
        
        // Need ability to start refinement
        var student = _cadences!.All.First(c => c.Name == "Student");
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockAbility("Student", "Refine Fire");
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        _resourceManager.Inventory.Add(refData.InputItem, refData.Recipe.InputQuantity);
        _resourceManager.StartQuest(refData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        // Remove assignment -> loses ability
        _resourceManager.JunctionManager.Unassign(student, _resourceManager.UnlockedAbilities);
        
        // Reevaluate should cancel the refinement
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ResourceManager_IsInProgress_Works()
    {
        var character = _resourceManager!.Characters[0];
        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        
        Assert.IsFalse(_resourceManager.IsInProgress(q1));
        
        _resourceManager.StartQuest(q1, character);
        Assert.IsTrue(_resourceManager.IsInProgress(q1));
    }

    [TestMethod]
    public void StartQuest_PreventsDuplicateInProgress()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should not start duplicate single-use quest.");
    }

    [TestMethod]
    public void StartQuest_PreventsCompletedSingleUse()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        
        _resourceManager.ReceiveRewards(prologue).Wait();
        Assert.IsTrue(_resourceManager.GetCompletedQuests().Contains(prologue.Quest));
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Should not start completed single-use quest.");
    }

    [TestMethod]
    public void StartQuest_CadenceUnlock_Works()
    {
        var character = _resourceManager!.Characters[0];
        var arcanist = _cadences!.All.First(c => c.Name == "Arcanist");
        var unlock = arcanist.Abilities.First(a => a.Ability.Name == "Refine Ice");
        
        // Find requirements for Refine Ice in Arcanist
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        
        _resourceManager.StartQuest(unlock, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.IsTrue(_resourceManager.ActiveQuests[0].Item is CadenceUnlock);
    }

    [TestMethod]
    public void ResourceManager_RefundCosts_CadenceUnlock_Works()
    {
        var character = _resourceManager!.Characters[0];
        var arcanist = _cadences!.All.First(c => c.Name == "Arcanist");
        var unlock = arcanist.Abilities.First(a => a.Ability.Name == "Refine Ice");
        
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        var progress = _resourceManager.ActiveQuests[0];
        
        _resourceManager.CancelQuest(progress);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
        foreach(var req in unlock.Requirements) Assert.AreEqual(req.Quantity, _resourceManager.Inventory.GetQuantity(req.Item));
    }

    [TestMethod]
    public void ResourceManager_RefundCosts_RefinementData_Works()
    {
        var character = _resourceManager!.Characters[0];
        var refData = _resourceManager.Refinements.GetRefinement("Refine Fire", "Basic Gem")!.Value;
        
        // Need ability to start refinement
        var student = _cadences!.All.First(c => c.Name == "Student");
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockAbility("Student", "Refine Fire");
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        _resourceManager.Inventory.Add(refData.InputItem, refData.Recipe.InputQuantity);
        _resourceManager.StartQuest(refData, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);
        
        Assert.AreEqual(refData.Recipe.InputQuantity, _resourceManager.Inventory.GetQuantity(refData.InputItem));
    }

    [TestMethod]
    public void ResourceManager_ToggleRecipeStar_Works()
    {
        _resourceManager!.ToggleRecipeStar("TestKey");
        Assert.IsTrue(_resourceManager.StarredRecipes.Contains("TestKey"));
        _resourceManager.ToggleRecipeStar("TestKey");
        Assert.IsFalse(_resourceManager.StarredRecipes.Contains("TestKey"));
    }

    [TestMethod]
    public void ResourceManager_HasAbility_Works()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        var ability = recruit.Abilities.First(a => a.Ability.Name == "AutoQuest I").Ability;
        
        Assert.IsFalse(_resourceManager.HasAbility(character, ability));
        
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility("Recruit", "AutoQuest I");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        
        Assert.IsTrue(_resourceManager.HasAbility(character, ability));
    }
}
