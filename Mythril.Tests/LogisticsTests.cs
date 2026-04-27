using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class LogisticsTests
{
    private ResourceManager? _resourceManager;
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

        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), _cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            _cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(gameStore, _items, _quests,
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
    public void Character_DefaultTaskLimit_IsOne()
    {
        var character = _resourceManager!.Characters[0];
        Assert.AreEqual(1, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_WithLogisticsI_TaskLimit_IsTwo()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == SandboxContent.Weaver);

        // Unlock and assign
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockAbility(SandboxContent.Weaver, SandboxContent.LogisticsI);
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(2, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_WithLogisticsII_TaskLimit_IsThree()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == SandboxContent.Scholar);

        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.LogisticsII);
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(3, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_CannotExceedTaskLimit()
    {
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == SandboxContent.Gold);
        _resourceManager.Inventory.Add(gold, 1000);

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.Prologue), _questDetails![_quests.All.First(q => q.Name == SandboxContent.Prologue)]);
        var quest2 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.Tutorial), _questDetails[_quests.All.First(q => q.Name == SandboxContent.Tutorial)]);

        // Limit is 1
        _resourceManager.StartQuest(quest1, character);
        Assert.ContainsSingle(q => q.Character.Name == character.Name, _resourceManager.ActiveQuests);

        _resourceManager.StartQuest(quest2, character);
        Assert.ContainsSingle(q => q.Character.Name == character.Name, _resourceManager.ActiveQuests, "Should not allow second quest.");
    }

    [TestMethod]
    public void LosingLogisticsI_CancelsAndRefundsExcessTask()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == SandboxContent.Weaver);
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockAbility(SandboxContent.Weaver, SandboxContent.LogisticsI);
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        var gold = _items!.All.First(x => x.Name == SandboxContent.Gold);
        var iron = _items.All.First(x => x.Name == SandboxContent.IronOre);

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);
        _resourceManager.Inventory.Add(iron, 100);

        // Mark prerequisites done
        _resourceManager.RestoreCompletedQuest(_quests!.All.First(q => q.Name == SandboxContent.Prologue));
        _resourceManager.RestoreCompletedQuest(_quests.All.First(q => q.Name == SandboxContent.Tutorial));

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.BuyPotion), _questDetails![_quests.All.First(q => q.Name == SandboxContent.BuyPotion)]);
        var questSpark = new QuestData(_quests.All.First(q => q.Name == SandboxContent.RekindlingTheSpark), _questDetails[_quests.All.First(q => q.Name == SandboxContent.RekindlingTheSpark)]);

        _resourceManager.StartQuest(quest1, character);
        _resourceManager.StartQuest(questSpark, character);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));

        var ironAfterStart = _resourceManager.Inventory.GetQuantity(iron);

        // Unequip weaver -> Lose Logistics I -> Should cancel questSpark (last added)
        _resourceManager.JunctionManager.Unassign(weaver, _resourceManager.UnlockedAbilities);

        Assert.ContainsSingle(q => q.Character.Name == character.Name, _resourceManager.ActiveQuests);
        // BuyPotion costs 100 Gold. RekindlingTheSpark costs 10 Iron Ore.
        Assert.AreEqual(ironAfterStart + 10, _resourceManager.Inventory.GetQuantity(iron), "Second quest SHOULD be refunded.");
    }

    [TestMethod]
    public void StartQuest_PreventsCompletedSingleUseLooping()
    {
        var character = _resourceManager!.Characters[0];
        var quest = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.Prologue), _questDetails![_quests.All.First(q => q.Name == SandboxContent.Prologue)]);

        // Complete it
        _resourceManager.ReceiveRewards(quest).Wait(TestContext.CancellationToken);
        Assert.IsTrue(_resourceManager.GetCompletedQuests().Contains(quest.Quest));

        // Try to start it again
        _resourceManager.StartQuest(quest, character);

        Assert.IsEmpty(_resourceManager.ActiveQuests, "Should not be able to start a completed single-use quest.");
    }

    [TestMethod]
    public void Character_TaskLimit_SlotAllocation_Correctness()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == SandboxContent.Scholar);

        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.LogisticsII);
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);

        // Mark prerequisites done for Buy Potion
        _resourceManager.RestoreCompletedQuest(_quests!.All.First(q => q.Name == SandboxContent.Prologue));
        _resourceManager.RestoreCompletedQuest(_quests.All.First(q => q.Name == SandboxContent.Tutorial));

        var questData = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.BuyPotion), _questDetails![_quests.All.First(q => q.Name == SandboxContent.BuyPotion)]);
        _resourceManager.Inventory.Add(_items!.All.First(i => i.Name == SandboxContent.Gold), 1000);

        // Fill slots
        _resourceManager.StartQuest(questData, character); // Slot 0
        _resourceManager.StartQuest(questData, character); // Slot 1
        _resourceManager.StartQuest(questData, character); // Slot 2

        var active = _resourceManager.ActiveQuests.Where(q => q.Character.Name == character.Name).ToList();
        Assert.HasCount(3, active);
        Assert.Contains(a => a.SlotIndex == 0, active);
        Assert.Contains(a => a.SlotIndex == 1, active);
        Assert.Contains(a => a.SlotIndex == 2, active);
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RefundsCorrectly()
    {
        var character = _resourceManager!.Characters[0];
        // Mark prerequisites done
        _resourceManager.RestoreCompletedQuest(_quests!.All.First(q => q.Name == SandboxContent.Prologue));
        _resourceManager.RestoreCompletedQuest(_quests.All.First(q => q.Name == SandboxContent.Tutorial));

        var questData = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.BuyPotion), _questDetails![_quests.All.First(q => q.Name == SandboxContent.BuyPotion)]);
        var gold = _items!.All.First(i => i.Name == SandboxContent.Gold);

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 250);

        _resourceManager.StartQuest(questData, character);
        // Quantity should be 150 here because quest costs 100 gold
        Assert.AreEqual(150, _resourceManager.Inventory.GetQuantity(gold));

        var active = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(active);

        // Should be 250 now after refund
        Assert.AreEqual(250, _resourceManager.Inventory.GetQuantity(gold), "Gold should be fully refunded on cancellation.");
    }

    public TestContext TestContext { get; set; }
}