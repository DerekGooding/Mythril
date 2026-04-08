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
        
        var inventory = new InventoryManager();
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails,
            cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(
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
        
        _resourceManager.Inventory.Clear();
    }

    [TestMethod]
    public void ResourceManager_Inventory_IsAccessible()
    {
        Assert.IsNotNull(_resourceManager?.Inventory);
    }
}
