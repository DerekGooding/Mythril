using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Mythril.Tests;

[TestClass]
public class InventoryOverflowTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private InventoryManager? _inventory;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        var gameStore = new GameStore();
        _inventory = new InventoryManager(gameStore);
        
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(gameStore, _inventory, ContentHost.GetContent<StatAugments>(), cadences);
        
        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestDetails>(),
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(
            gameStore,
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            _inventory,
            ContentHost.GetContent<ItemRefinements>(),
            pathfinding);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void InventoryManager_Add_ReturnsOverflow_WhenCapacityReached()
    {
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");
        _inventory!.MagicCapacity = 30;

        // 1. Add up to 25 (No overflow)
        int overflow = _inventory.Add(fireMagic, 25);
        Assert.AreEqual(0, overflow);
        Assert.AreEqual(25, _inventory.GetQuantity(fireMagic));

        // 2. Add 10 more (Overflow 5)
        overflow = _inventory.Add(fireMagic, 10);
        Assert.AreEqual(5, overflow);
        Assert.AreEqual(30, _inventory.GetQuantity(fireMagic));
    }

    [TestMethod]
    public async Task ReceiveRewards_TriggersOverflowEvent_ForQuests()
    {
        var gold = _items!.All.First(i => i.Name == "Gold");
        var quest = ContentHost.GetContent<Quests>().All.First(q => q.Name == "Prologue");
        var detail = ContentHost.GetContent<QuestDetails>()[quest];
        // Ensure prologue rewards gold
        var questData = new QuestData(quest, detail);

        string? overflowItem = null;
        int overflowQty = 0;
        _resourceManager!.OnItemOverflow += (name, qty) => 
        {
            overflowItem = name;
            overflowQty = qty;
        };

        // Quests don't currently have capacity limits (Gold is unlimited), 
        // but let's test if we added a spell reward.
        // Let's mock a quest reward that is a spell.
        var spell = _items.All.First(i => i.Name == "Fire I");
        var customDetail = new QuestDetail(1, [], [new ItemQuantity(spell, 50)], QuestType.Single);
        var customQuestData = new QuestData(quest, customDetail);

        _inventory!.MagicCapacity = 30;
        await _resourceManager.ReceiveRewards(customQuestData);

        Assert.AreEqual("Fire I", overflowItem);
        Assert.AreEqual(20, overflowQty); // 50 - 30 = 20
    }

    [TestMethod]
    public async Task ReceiveRewards_TriggersOverflowEvent_ForRefinements()
    {
        var basicGem = _items!.All.First(i => i.Name == "Basic Gem");
        var fireI = _items.All.First(i => i.Name == "Fire I");
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        
        // Find the specific ability object from the cadence content
        var ability = student.Abilities.First(a => a.Ability.Name == "Refine Fire").Ability;
        var recipe = ContentHost.GetContent<ItemRefinements>().ByKey[ability].Recipes[basicGem];
        // Recipe produces 5x Fire I
        
        var refinement = new RefinementData(ability, basicGem, recipe, "Magic");

        string? overflowItem = null;
        _resourceManager!.OnItemOverflow += (name, qty) => overflowItem = name;

        _inventory!.MagicCapacity = 2; // Very low capacity
        await _resourceManager.ReceiveRewards(refinement);

        Assert.AreEqual("Fire I", overflowItem);
    }
}
