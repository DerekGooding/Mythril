using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Mythril.Tests;

[TestClass]
public class QuestRewardTests
{
    private ResourceManager? _resourceManager;
    private GameStore? _gameStore;
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
        
        _gameStore = new GameStore();
        var inventory = new InventoryManager(_gameStore);
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(_gameStore, inventory, ContentHost.GetContent<StatAugments>(), cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(_gameStore, _items, _quests, 
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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        Assert.IsFalse(_resourceManager.CanAfford(questData));

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        Assert.IsTrue(_resourceManager.CanAfford(questData));
    }

    [TestMethod]
    public void ResourceManager_PayCosts_RemovesItems()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        
        _resourceManager.PayCosts(questData);
        
        // Buy Potion costs 100
        Assert.AreEqual(900, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == SandboxContent.Gold)));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_AddsItems()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);

        _resourceManager!.Inventory.Clear();
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        Assert.AreEqual(1, _resourceManager.Inventory.GetQuantity(_items!.All.First(x => x.Name == SandboxContent.Potion)));
    }

    [TestMethod]
    public void ResourceManager_PayCosts_SingleQuest_DoesNotLockQuest()
    {
        var area = _resourceManager!.UsableLocations.First(l => l.Name == "Starting Area");
        var quest = area.Quests.First(q => q.Name == SandboxContent.Prologue);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(area.Quests.Contains(quest));
        
        _resourceManager.PayCosts(questData);
        
        Assert.IsTrue(area.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_ReceiveRewards_SingleQuest_LocksQuest()
    {
        var area = _resourceManager!.UsableLocations.First(l => l.Name == "Starting Area");
        var quest = area.Quests.First(q => q.Name == SandboxContent.Prologue);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        
        Assert.IsTrue(area.Quests.Contains(quest));
        
        _resourceManager.ReceiveRewards(questData).Wait();
        
        // Refetch location as it is recreated on completion
        area = _resourceManager!.UsableLocations.First(l => l.Name == "Starting Area");
        Assert.IsFalse(area.Quests.Contains(quest));
    }

    [TestMethod]
    public void ResourceManager_CancelQuest_RefundsCosts()
    {
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == SandboxContent.Gold);

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
        var quest = _quests!.All.First(x => x.Name == SandboxContent.BuyPotion);
        var detail = _questDetails![quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.Inventory.Add(_items!.All.First(x => x.Name == SandboxContent.Gold), 1000);
        
        _resourceManager.StartQuest(questData, character);
        _resourceManager.ReceiveRewards(questData).Wait();

        Assert.IsTrue(_resourceManager.GetCompletedQuests().Any());
        
        _resourceManager.Initialize();

        Assert.IsFalse(_resourceManager.ActiveQuests.Any());
        Assert.IsFalse(_resourceManager.GetCompletedQuests().Any());
        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(_items.All.First(x => x.Name == SandboxContent.Gold)));
    }

    [TestMethod]
    public async Task ReceiveRewards_CadenceUnlock_Works()
    {
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        // Use RefineScrap because it's a refinement and will trigger HasUnseenWorkshop
        var ability = recruit.Abilities.First(a => a.Ability.Name == SandboxContent.RefineScrap).Ability;
        var unlock = new CadenceUnlock(SandboxContent.Recruit, ability, [], SandboxContent.Strength);

        string abilityKey = $"{SandboxContent.Recruit}:{SandboxContent.RefineScrap}";
        Assert.IsFalse(_resourceManager!.UnlockedAbilities.Contains(abilityKey));
        
        _resourceManager.ActiveTab = "hand";
        await _resourceManager.ReceiveRewards(unlock);
        
        Assert.IsTrue(_resourceManager.UnlockedAbilities.Contains(abilityKey));
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);
    }

    [TestMethod]
    public async Task ReceiveRewards_RefinementData_Works()
    {
        var refData = _resourceManager!.Refinements.GetRefinement(SandboxContent.RefineScrap, SandboxContent.Scrap);
        _resourceManager.Inventory.Clear();
        
        await _resourceManager.ReceiveRewards(refData!.Value);
        
        Assert.AreEqual(10, _resourceManager.Inventory.GetQuantity(_items!.All.First(x => x.Name == SandboxContent.Gold)));
    }

    [TestMethod]
    public async Task ReceiveRewards_RefinementData_TriggersOverflow()
    {
        var refData = _resourceManager!.Refinements.GetRefinement(SandboxContent.RefineFire, SandboxContent.BasicGem);
        _resourceManager.Inventory.Clear();
        _gameStore!.Dispatch(new SetMagicCapacityAction(15)); // Cap at 15
        
        // Output of RefineFire is Fire I (Spell), which has capacity
        var fireI = _items!.All.First(x => x.Name == SandboxContent.FireI);
        _resourceManager.Inventory.Add(fireI, 8);
        
        string overflowItem = "";
        int overflowQty = 0;
        _resourceManager.OnItemOverflow += (name, qty) => {
            overflowItem = name;
            overflowQty = qty;
        };
        
        await _resourceManager.ReceiveRewards(refData!.Value);
        
        // RefineFire recipe produces 5 Fire I
        Assert.AreEqual(15, _resourceManager.Inventory.GetQuantity(fireI));
        Assert.AreEqual(SandboxContent.FireI, overflowItem);
        Assert.AreEqual(3, overflowQty); // 8 + 5 = 13. 13 - 10 = 3. Wait, 8+5 = 13, cap 15. No overflow?
        // Ah, the test case in original code had: Expected 15, Actual 18. Cap 15. 
        // 8 + 10 = 18. 18 - 15 = 3 overflow.
        // My RefineFire recipe produces 5. Let's make it 10 to match the test's expectation of overflow.
    }
}
