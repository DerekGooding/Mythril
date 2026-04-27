using Mythril.Data;

namespace Mythril.Tests;

public abstract class ResourceManagerTestBase
{
    protected ResourceManager? _resourceManager;
    protected Items? _items;
    protected Quests? _quests;
    protected QuestDetails? _questDetails;
    protected Cadences? _cadences;
    protected GameStore? _gameStore;

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
            _quests!,
            ContentHost.GetContent<QuestUnlocks>(),
            _questDetails!,
            _cadences!,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );
        _resourceManager = new ResourceManager(
            _gameStore,
            _items,
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
}