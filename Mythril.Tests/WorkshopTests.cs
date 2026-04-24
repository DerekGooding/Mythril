using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class WorkshopTests
{
    private ResourceManager? _resourceManager;
    private ItemRefinements? _refinements;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        SandboxContent.Load();
        _refinements = ContentHost.GetContent<ItemRefinements>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        var junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), _cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestDetails>(),
            _cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        _resourceManager = new ResourceManager(gameStore, ContentHost.GetContent<Items>(), ContentHost.GetContent<Quests>(), 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            _refinements,
            pathfinding);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void LearningRefinementAbility_UnlocksWorkshop_AndSetsUnseen()
    {
        // 1. Find a cadence with a refinement ability
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var refineFire = apprentice.Abilities.First(a => a.Ability.Name == SandboxContent.RefineFire);

        // 2. Initially, it shouldn't be unlocked
        string abilityKey = $"{SandboxContent.Apprentice}:{SandboxContent.RefineFire}";
        Assert.IsFalse(_resourceManager!.UnlockedAbilities.Contains(abilityKey));
        Assert.IsFalse(_resourceManager.HasUnseenWorkshop);
        _resourceManager.ActiveTab = "hand";

        // 3. Receive rewards (unlock it)
        _resourceManager.ReceiveRewards(refineFire).Wait();

        // 4. Assert it's unlocked and flagged as unseen
        Assert.IsTrue(_resourceManager.UnlockedAbilities.Contains(abilityKey));
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);
    }

    [TestMethod]
    public void LearningSameRefinementAbilityTwice_DoesNotReTriggerUnseen()
    {
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var refineFire = apprentice.Abilities.First(a => a.Ability.Name == SandboxContent.RefineFire);

        _resourceManager!.ActiveTab = "hand";
        // First unlock
        _resourceManager!.ReceiveRewards(refineFire).Wait();
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);
        _resourceManager.HasUnseenWorkshop = false;

        // Second unlock (e.g. from a different cadence if it had it, or just re-running)
        _resourceManager.ReceiveRewards(refineFire).Wait();
        
        // Should NOT be unseen again if already known
        Assert.IsFalse(_resourceManager.HasUnseenWorkshop);
    }
}
