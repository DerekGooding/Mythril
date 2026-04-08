using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Mythril.Tests;

[TestClass]
public class AutomationTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private Quests? _quests;
    private QuestDetails? _questDetails;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), _cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            _cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(
            _items, 
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
    public async Task AutoQuest_RestartsSlotZero()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockedAbilities.Add("Recruit:AutoQuest I");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var questGoblins = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        _resourceManager.StartQuest(questGoblins, character); 

        var progress = _resourceManager.ActiveQuests.First();
        await _resourceManager.CompleteTaskAsync(progress);

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Slot 0 SHOULD have restarted.");
        Assert.AreEqual(0, _resourceManager.ActiveQuests.First().SlotIndex);
    }

    [TestMethod]
    public async Task AutoQuestII_RestartsSlotOne()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockedAbilities.Add("Scholar:Logistics II");
        _resourceManager.UnlockedAbilities.Add("Scholar:AutoQuest II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == "Hunt Bats"), _questDetails[_quests.All.First(q => q.Name == "Hunt Bats")]);

        _resourceManager.StartQuest(q1, character); // Slot 0
        _resourceManager.StartQuest(q2, character); // Slot 1

        var progress1 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 1);
        await _resourceManager.CompleteTaskAsync(progress1);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count, "Slot 1 SHOULD have restarted with AutoQuest II.");
        Assert.IsTrue(_resourceManager.ActiveQuests.Any(q => q.SlotIndex == 1));
    }

    [TestMethod]
    public async Task SlotTwo_NeverAutoRestarts()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockedAbilities.Add("Scholar:Logistics II");
        _resourceManager.UnlockedAbilities.Add("Scholar:AutoQuest II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == "Hunt Bats"), _questDetails[_quests.All.First(q => q.Name == "Hunt Bats")]);
        var q3 = new QuestData(_quests.All.First(q => q.Name == "Hunt Spiders"), _questDetails[_quests.All.First(q => q.Name == "Hunt Spiders")]);

        _resourceManager.StartQuest(q1, character); 
        _resourceManager.StartQuest(q2, character);
        _resourceManager.StartQuest(q3, character); // Slot 2

        var progress2 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 2);
        await _resourceManager.CompleteTaskAsync(progress2);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count, "Slot 2 should NOT have restarted.");
    }

    [TestMethod]
    public async Task AutoQuest_DoesNotRestart_SingleUseQuest()
    {
        var character = _resourceManager!.Characters[0];
        _resourceManager.UnlockedAbilities.Add("Recruit:AutoQuest I");
        _resourceManager.SetAutoQuestEnabled(character, true);

        var prologue = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        _resourceManager.StartQuest(prologue, character);

        var progress = _resourceManager.ActiveQuests.First();
        await _resourceManager.CompleteTaskAsync(progress);

        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Single-use quest should NOT auto-restart.");
    }

    [TestMethod]
    public async Task Refinement_AutoQuest_RespectsMagicCapacity()
    {
        var character = _resourceManager!.Characters[0];
        var student = _cadences!.All.First(c => c.Name == "Student");
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockedAbilities.Add("Student:AutoQuest I");
        _resourceManager.UnlockedAbilities.Add("Student:Refine Fire");
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Get "Refine Fire" refinement for "Basic Gem" input
        var refData = _resourceManager.Refinements.GetRefinement("Refine Fire", "Basic Gem");
        Assert.IsNotNull(refData, "Refinement 'Refine Fire' for 'Basic Gem' should exist.");
        
        // Set capacity to 30
        _resourceManager.Inventory.MagicCapacity = 30;
        // Fill inventory to capacity
        _resourceManager.Inventory.Add(refData.Value.Recipe.OutputItem, 30);
        // Add input item
        _resourceManager.Inventory.Add(refData.Value.InputItem, 1);

        _resourceManager.StartQuest(refData.Value, character);
        var progress = _resourceManager.ActiveQuests.First();
        await _resourceManager.CompleteTaskAsync(progress);

        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Refinement producing Magic should NOT restart when capacity reached.");
    }
}
