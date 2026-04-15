using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JournalTests
{
    private ResourceManager? _resourceManager;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var items = ContentHost.GetContent<Items>();
        var quests = ContentHost.GetContent<Quests>();
        var questDetails = ContentHost.GetContent<QuestDetails>();
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), cadences);
        var pathfinding = new PathfindingService(ContentHost.GetContent<Locations>(), quests, ContentHost.GetContent<QuestUnlocks>(), questDetails, cadences, ContentHost.GetContent<QuestToCadenceUnlocks>());

        _resourceManager = new ResourceManager(gameStore, items, quests, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            questDetails, 
            cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>(),
            pathfinding);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public async Task Journal_AddEntry_Works()
    {
        var quest = ContentHost.GetContent<Quests>().All.First();
        var detail = ContentHost.GetContent<QuestDetails>()[quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests[0];
        
        await _resourceManager.ReceiveRewards(progress);

        Assert.AreEqual(1, _resourceManager.Journal.Count);
        Assert.AreEqual(quest.Name, _resourceManager.Journal[0].TaskName);
    }

    [TestMethod]
    public void Journal_Clear_Works()
    {
        _resourceManager!.Journal.Add(new ResourceManager.JournalEntry("Test", "Hero", "Details", DateTime.Now));
        Assert.AreEqual(1, _resourceManager.Journal.Count);
        
        _resourceManager.ClearJournal();
        Assert.AreEqual(0, _resourceManager.Journal.Count);
    }
}
