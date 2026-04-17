using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mythril.Tests;

[TestClass]
public class AutoQuestCancellationTests
{
    private ResourceManager? _resourceManager;
    private GameStore? _gameStore;
    private Quests? _quests;
    private QuestDetails? _questDetails;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();

        _gameStore = new GameStore();
        var items = ContentHost.GetContent<Items>();
        var inventory = new InventoryManager(_gameStore);
        var statAugments = ContentHost.GetContent<StatAugments>();
        var cadences = ContentHost.GetContent<Cadences>();
        var junctionManager = new JunctionManager(_gameStore, inventory, statAugments, cadences);
        
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        var questUnlocks = ContentHost.GetContent<QuestUnlocks>();
        var questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();
        var locations = ContentHost.GetContent<Locations>();
        var refinements = ContentHost.GetContent<ItemRefinements>();
        var pathfinding = new PathfindingService(locations, _quests, questUnlocks, _questDetails, cadences, questToCadenceUnlocks);

        _resourceManager = new ResourceManager(
            _gameStore, 
            items,
            _quests, 
            questUnlocks, 
            questToCadenceUnlocks, 
            _questDetails, 
            cadences, 
            locations,
            junctionManager,
            inventory,
            refinements,
            pathfinding);
        
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void CancelledQuest_DoesNotAutoRestart_EvenIfAutoQuestEnabled()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        var recurringQuest = _quests!.All.First(q => _questDetails![q].Type == QuestType.Recurring);
        var questData = new QuestData(recurringQuest, _questDetails![recurringQuest]);

        // Enable AutoQuest (mocking the ability unlock)
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Recruit");
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility("Recruit", "AutoQuest I");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Act: Start and then Cancel the quest
        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests.First(p => p.Character.Name == character.Name);
        _resourceManager.CancelQuest(progress);

        // Simulate a tick to trigger CheckAutoQuestTick
        _resourceManager.Tick(1.0);

        // Assert
        var activeAfterTick = _resourceManager.ActiveQuests.Where(p => p.Character.Name == character.Name).ToList();
        Assert.AreEqual(0, activeAfterTick.Count, "Quest should not have auto-restarted after cancellation.");
    }

    [TestMethod]
    public async Task CompletedQuest_DoesAutoRestart_WhenAutoQuestEnabled()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        var recurringQuest = _quests!.All.First(q => _questDetails![q].Type == QuestType.Recurring);
        var questData = new QuestData(recurringQuest, _questDetails![recurringQuest]);

        // Enable AutoQuest
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Recruit");
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility("Recruit", "AutoQuest I");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Act: Start and complete the quest
        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests.First(p => p.Character.Name == character.Name);
        
        // Complete the quest by ticking
        _resourceManager.Tick(questData.DurationSeconds + 1);

        // Simulation usually calls ReceiveRewards which finishes the quest
        await _resourceManager.ReceiveRewards(progress);

        // Simulate another tick to trigger CheckAutoQuestTick
        _resourceManager.Tick(1.0);

        // Assert
        var activeAfterTick = _resourceManager.ActiveQuests.Where(p => p.Character.Name == character.Name).ToList();
        Assert.IsTrue(activeAfterTick.Any(), "Quest should have auto-restarted after completion.");
        Assert.AreEqual(recurringQuest.Name, activeAfterTick[0].Name);
    }
}
