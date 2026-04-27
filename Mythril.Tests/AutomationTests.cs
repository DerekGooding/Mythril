using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class AutomationTests
{
    private ResourceManager? _resourceManager;
    private GameStore? _gameStore;
    private Items? _items;
    private Quests? _quests;
    private QuestDetails? _questDetails;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        SandboxContent.Load();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        _cadences = ContentHost.GetContent<Cadences>();

        _gameStore = new GameStore();
        var inventory = new InventoryManager(_gameStore);
        var junctionManager = new JunctionManager(_gameStore, inventory, ContentHost.GetContent<StatAugments>(), _cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            _cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(_gameStore, _items, _quests,
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
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var questGoblins = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.HuntGoblins), _questDetails![_quests.All.First(q => q.Name == SandboxContent.HuntGoblins)]);
        _resourceManager.StartQuest(questGoblins, character);

        var progress = _resourceManager.ActiveQuests[0];
        await _resourceManager.ReceiveRewards(progress);

        Assert.HasCount(1, _resourceManager.ActiveQuests, "Slot 0 SHOULD have restarted.");
        Assert.AreEqual(0, _resourceManager.ActiveQuests[0].SlotIndex);
    }

    [TestMethod]
    public async Task AutoQuestII_RestartsSlotOne()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == SandboxContent.Scholar);
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.LogisticsII);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.AutoQuestII);
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var q1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.HuntGoblins), _questDetails![_quests.All.First(q => q.Name == SandboxContent.HuntGoblins)]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.HuntBats), _questDetails[_quests.All.First(q => q.Name == SandboxContent.HuntBats)]);

        _resourceManager.StartQuest(q1, character); // Slot 0
        _resourceManager.StartQuest(q2, character); // Slot 1

        var progress1 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 1);
        await _resourceManager.ReceiveRewards(progress1);

        Assert.HasCount(2, _resourceManager.ActiveQuests, "Slot 1 SHOULD have restarted with AutoQuest II.");
        Assert.Contains(q => q.SlotIndex == 1, _resourceManager.ActiveQuests);
    }

    [TestMethod]
    public async Task SlotTwo_NeverAutoRestarts()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == SandboxContent.Scholar);
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.LogisticsII);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.AutoQuestII);
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var q1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.HuntGoblins), _questDetails![_quests.All.First(q => q.Name == SandboxContent.HuntGoblins)]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.HuntBats), _questDetails[_quests.All.First(q => q.Name == SandboxContent.HuntBats)]);
        var q3 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.HuntSpiders), _questDetails[_quests.All.First(q => q.Name == SandboxContent.HuntSpiders)]);

        _resourceManager.StartQuest(q1, character);
        _resourceManager.StartQuest(q2, character);
        _resourceManager.StartQuest(q3, character); // Slot 2

        var progress2 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 2);
        await _resourceManager.ReceiveRewards(progress2);

        Assert.HasCount(2, _resourceManager.ActiveQuests, "Slot 2 should NOT have restarted.");
    }

    [TestMethod]
    public async Task AutoQuest_DoesNotRestart_SingleUseQuest()
    {
        var character = _resourceManager!.Characters[0];
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var prologue = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.Prologue), _questDetails![_quests.All.First(q => q.Name == SandboxContent.Prologue)]);
        _resourceManager.StartQuest(prologue, character);

        var progress = _resourceManager.ActiveQuests[0];
        await _resourceManager.ReceiveRewards(progress);

        Assert.IsEmpty(_resourceManager.ActiveQuests, "Single-use quest should NOT auto-restart.");
    }

    [TestMethod]
    public async Task Refinement_AutoQuest_RespectsMagicCapacity()
    {
        var character = _resourceManager!.Characters[0];
        var student = _cadences!.All.First(c => c.Name == SandboxContent.Student);
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.AutoQuestI);
        _resourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.RefineFire);
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Get "Refine Fire" refinement for "Basic Gem" input
        var refData = _resourceManager.Refinements.GetRefinement(SandboxContent.RefineFire, SandboxContent.BasicGem);
        Assert.IsNotNull(refData, "Refinement 'Refine Fire' for 'Basic Gem' should exist.");

        // Set capacity to 30
        _gameStore!.Dispatch(new SetMagicCapacityAction(30));
        // Fill inventory to near capacity (29/30)
        _resourceManager.Inventory.Add(refData.Value.Recipe.OutputItem, 29);
        // Add input item
        _resourceManager.Inventory.Add(refData.Value.InputItem, 1);

        _resourceManager.StartQuest(refData.Value, character);
        var progress = _resourceManager.ActiveQuests[0];
        await _resourceManager.ReceiveRewards(progress);

        Assert.IsEmpty(_resourceManager.ActiveQuests, "Refinement producing Magic should NOT restart when capacity reached.");
    }
}