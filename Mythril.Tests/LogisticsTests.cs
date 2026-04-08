using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;

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
        TestContentLoader.Load();
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

        _resourceManager = new ResourceManager(
            gameStore,
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
    public void Character_DefaultTaskLimit_IsOne()
    {
        var character = _resourceManager!.Characters[0];
        Assert.AreEqual(1, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_WithLogisticsI_TaskLimit_IsTwo()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        
        // Unlock and assign
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(2, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_WithLogisticsII_TaskLimit_IsThree()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockedAbilities.Add("Scholar:Logistics II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(3, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_CannotExceedTaskLimit()
    {
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == "Gold");
        _resourceManager.Inventory.Add(gold, 1000);

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        var quest2 = new QuestData(_quests.All.First(q => q.Name == "Visit Starting Town"), _questDetails[_quests.All.First(q => q.Name == "Visit Starting Town")]);

        // Limit is 1
        _resourceManager.StartQuest(quest1, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));

        _resourceManager.StartQuest(quest2, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name), "Should not allow second quest.");
    }

    [TestMethod]
    public void LosingLogisticsI_CancelsAndRefundsExcessTask()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        var gold = _items!.All.First(x => x.Name == "Gold");
        var iron = _items.All.First(x => x.Name == "Iron Ore");
        
        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);
        _resourceManager.Inventory.Add(iron, 100);

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == "Buy Potion"), _questDetails![_quests.All.First(q => q.Name == "Buy Potion")]); 
        
        // "Rekindling the Spark" costs Iron Ore
        var questSpark = new QuestData(_quests.All.First(q => q.Name == "Rekindling the Spark"), _questDetails[_quests.All.First(q => q.Name == "Rekindling the Spark")]); 
        var bark = _items.All.First(x => x.Name == "Ancient Bark");
        _resourceManager.Inventory.Add(bark, 10);

        _resourceManager.StartQuest(quest1, character);
        _resourceManager.StartQuest(questSpark, character);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));
        
        int ironAfterStart = _resourceManager.Inventory.GetQuantity(iron);

        // Unequip weaver -> Lose Logistics I -> Should cancel questSpark (last added)
        _resourceManager.JunctionManager.Unassign(weaver, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));
        Assert.AreEqual(ironAfterStart + 20, _resourceManager.Inventory.GetQuantity(iron), "Second quest SHOULD be refunded.");
    }

    [TestMethod]
    public void StartQuest_PreventsCompletedSingleUseLooping()
    {
        var character = _resourceManager!.Characters[0];
        var quest = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);

        // Complete it
        _resourceManager.ReceiveRewards(quest).Wait();
        Assert.IsTrue(_resourceManager.GetCompletedQuests().Contains(quest.Quest));

        // Try to start it again
        _resourceManager.StartQuest(quest, character);

        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Should not be able to start a completed single-use quest.");
    }

    [TestMethod]
    public void Character_TaskLimit_SlotAllocation_Correctness()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockedAbilities.Add("Scholar:Logistics II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);

        var questData = new QuestData(_quests!.All.First(q => q.Name == "Buy Potion"), _questDetails![_quests.All.First(q => q.Name == "Buy Potion")]);
        _resourceManager.Inventory.Add(_items!.All.First(i => i.Name == "Gold"), 1000);
        _resourceManager.JunctionManager.AddStatBoost(character, "Speed", 10); // Meet requirement

        // Fill slots
        _resourceManager.StartQuest(questData, character); // Slot 0
        _resourceManager.StartQuest(questData, character); // Slot 1
        _resourceManager.StartQuest(questData, character); // Slot 2

        var active = _resourceManager.ActiveQuests.Where(q => q.Character.Name == character.Name).ToList();
        Assert.AreEqual(3, active.Count);
        Assert.IsTrue(active.Any(a => a.SlotIndex == 0));
        Assert.IsTrue(active.Any(a => a.SlotIndex == 1));
        Assert.IsTrue(active.Any(a => a.SlotIndex == 2));
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RefundsCorrectly()
    {
        var character = _resourceManager!.Characters[0];
        var questData = new QuestData(_quests!.All.First(q => q.Name == "Buy Potion"), _questDetails![_quests.All.First(q => q.Name == "Buy Potion")]);
        var gold = _items!.All.First(i => i.Name == "Gold");
        
        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 250);
        _resourceManager.JunctionManager.AddStatBoost(character, "Speed", 10); // Meet 15 Speed requirement
        
        // Unlock 'Buy Potion' by completing its prerequisites
        var prologue = _quests.All.First(q => q.Name == "Prologue");
        var tutorial = _quests.All.First(q => q.Name == "Tutorial Section");
        var town = _quests.All.First(q => q.Name == "Visit Starting Town");
        _resourceManager.ReceiveRewards(new QuestData(prologue, _questDetails[prologue])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(tutorial, _questDetails[tutorial])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(town, _questDetails[town])).Wait();

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 150);

        _resourceManager.StartQuest(questData, character);
        // Quantity should be 50 here because quest costs 100 gold
        Assert.AreEqual(50, _resourceManager.Inventory.GetQuantity(gold));

        var active = _resourceManager.ActiveQuests.First();
        _resourceManager.CancelQuest(active);

        // Should be 150 now after refund
        Assert.AreEqual(150, _resourceManager.Inventory.GetQuantity(gold), "Gold should be fully refunded on cancellation.");
    }
}
