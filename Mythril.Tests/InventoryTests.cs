using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private QuestDetails? _questDetails;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        var quests = ContentHost.GetContent<Quests>();

        _resourceManager = new ResourceManager(gameStore, _items, quests, 
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
        
        _resourceManager.Inventory.Clear();
    }

    [TestMethod]
    public void ResourceManager_Inventory_IsAccessible()
    {
        Assert.IsNotNull(_resourceManager?.Inventory);
    }

    [TestMethod]
    public void Inventory_AddAndRemove_Works()
    {
        var item = _items!.All.First(i => i.Name == "Gold");
        _resourceManager!.Inventory.Add(item, 10);
        Assert.AreEqual(10, _resourceManager.Inventory.GetQuantity(item));
        
        _resourceManager.Inventory.Remove(item, 4);
        Assert.AreEqual(6, _resourceManager.Inventory.GetQuantity(item));
    }

    [TestMethod]
    public void Inventory_Has_Works()
    {
        var item = _items!.All.First(i => i.Name == "Gold");
        _resourceManager!.Inventory.Add(item, 5);
        Assert.IsTrue(_resourceManager.Inventory.Has(item, 3));
        Assert.IsFalse(_resourceManager.Inventory.Has(item, 6));
    }

    [TestMethod]
    public void Inventory_GetAll_ReturnsCorrectItems()
    {
        var item1 = _items!.All[0];
        var item2 = _items!.All[1];
        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(item1, 5);
        _resourceManager.Inventory.Add(item2, 10);
        
        var all = _resourceManager.Inventory.GetAll().ToList();
        Assert.AreEqual(2, all.Count);
    }
}

