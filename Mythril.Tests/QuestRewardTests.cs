using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class QuestRewardTests
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

        _resourceManager = new ResourceManager(
            gameStore,
            _items, 
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
    public void ResourceManager_CanAfford_ReturnsCorrectValue()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(questData));

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        Assert.IsTrue(_resourceManager.CanAfford(questData));
    }

    [TestMethod]
    public void ResourceManager_PayCosts_RemovesItems()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        
        _resourceManager.PayCosts(questData);
        
        // Buy Potion costs 100
        Assert.AreEqual(900, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == "Gold")));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_AddsItems()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        Assert.AreEqual(5, _resourceManager.Inventory.GetQuantity(_items!.All.First(x => x.Name == "Potion")));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_Quest_UnlocksCadence()
    {
        var quest = _quests!.All.First(x => x.Name == "Ancient Inscriptions");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        _resourceManager!.ReceiveRewards(questData).Wait();
    }

    [TestMethod]
    public void ResourceManager_PayCosts_SingleQuest_DoesNotLockQuest()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var quest = village.Quests.First(q => q.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(village.Quests.Contains(quest));
        
        _resourceManager.PayCosts(questData);
        
        Assert.IsTrue(village.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_SingleQuest_LocksQuest()
    {
        var village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        var quest = village.Quests.First(q => q.Name == "Prologue");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(village.Quests.Contains(quest));
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        // Refetch location as it is recreated on completion
        village = _resourceManager!.UsableLocations.First(l => l.Name == "Village");
        Assert.IsFalse(village.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RefundsCosts()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == "Gold");

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);
        _resourceManager.JunctionManager.AddStatBoost(character, "Speed", 10); // Meet requirement
        
        // Prerequisites
        var prologue = _quests.All.First(q => q.Name == "Prologue");
        var tutorial = _quests.All.First(q => q.Name == "Tutorial Section");
        var town = _quests.All.First(q => q.Name == "Visit Starting Town");
        _resourceManager.ReceiveRewards(new QuestData(prologue, _questDetails[prologue])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(tutorial, _questDetails[tutorial])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(town, _questDetails[town])).Wait();

        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(900, _resourceManager.Inventory.GetQuantity(gold));

        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);

        Assert.AreEqual(1000, _resourceManager.Inventory.GetQuantity(gold));
    }

    [TestMethod]
    public void ResourceManager_Initialize_ClearsState()
    {
        var quest = _quests!.All.First(x => x.Name == "Buy Potion");
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Gold"), 1000);
        _resourceManager.JunctionManager.AddStatBoost(character, "Speed", 10);
        
        // Prerequisites
        var prologue = _quests.All.First(q => q.Name == "Prologue");
        var tutorial = _quests.All.First(q => q.Name == "Tutorial Section");
        var town = _quests.All.First(q => q.Name == "Visit Starting Town");
        _resourceManager.ReceiveRewards(new QuestData(prologue, _questDetails[prologue])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(tutorial, _questDetails[tutorial])).Wait();
        _resourceManager.ReceiveRewards(new QuestData(town, _questDetails[town])).Wait();

        _resourceManager.StartQuest(questData, character);
        _resourceManager.ReceiveRewards(questData).Wait();

        Assert.IsTrue(_resourceManager.GetCompletedQuests().Any());
        
        _resourceManager.Initialize();

        Assert.IsFalse(_resourceManager.ActiveQuests.Any());
        Assert.IsFalse(_resourceManager.GetCompletedQuests().Any());
        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == "Gold")));
    }

    [TestMethod]
    public async Task ReceiveRewards_CadenceUnlock_Works()
    {
        var arcanist = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Arcanist");
        var ability = arcanist.Abilities.First(a => a.Ability.Name == "Refine Ice").Ability;
        var unlock = new CadenceUnlock("Arcanist", ability, [], "Magic");

        Assert.IsFalse(_resourceManager!.UnlockedAbilities.Contains("Arcanist:Refine Ice"));
        
        await _resourceManager.ReceiveRewards(unlock);
        
        Assert.IsTrue(_resourceManager.UnlockedAbilities.Contains("Arcanist:Refine Ice"));
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);
    }

    [TestMethod]
    public async Task ReceiveRewards_RefinementData_Works()
    {
        var refData = _resourceManager!.Refinements.GetRefinement("Refine Fire", "Basic Gem");
        _resourceManager.Inventory.Clear();
        
        await _resourceManager.ReceiveRewards(refData!.Value);
        
        Assert.AreEqual(5, _resourceManager.Inventory.GetQuantity(_items!.All.First(x => x.Name == "Fire I")));
    }

    [TestMethod]
    public async Task ReceiveRewards_RefinementData_TriggersOverflow()
    {
        var refData = _resourceManager!.Refinements.GetRefinement("Refine Fire", "Basic Gem");
        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.MagicCapacity = 10;
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == "Fire I"), 8);
        
        string overflowItem = "";
        int overflowQty = 0;
        _resourceManager.OnItemOverflow += (name, qty) => {
            overflowItem = name;
            overflowQty = qty;
        };
        
        await _resourceManager.ReceiveRewards(refData!.Value);
        
        Assert.AreEqual(10, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == "Fire I")));
        Assert.AreEqual("Fire I", overflowItem);
        Assert.AreEqual(3, overflowQty); // 8 + 5 = 13. 13 - 10 = 3.
    }
}
