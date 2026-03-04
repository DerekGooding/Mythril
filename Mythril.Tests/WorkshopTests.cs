using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using Mythril.Blazor.Services;
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
        TestContentLoader.Load();
        _refinements = ContentHost.GetContent<ItemRefinements>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), _cadences);
        _resourceManager = new ResourceManager(
            ContentHost.GetContent<Items>(), 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            _refinements);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void LearningRefinementAbility_UnlocksWorkshop_AndSetsUnseen()
    {
        // 1. Find a cadence with a refinement ability
        var apprentice = _cadences!.All.First(c => c.Name == "Apprentice");
        var refineMixology = apprentice.Abilities.First(a => a.Ability.Name == "Refine Mixology");

        // 2. Initially, it shouldn't be unlocked
        Assert.IsFalse(_resourceManager!.UnlockedAbilities.Contains("Apprentice:Refine Mixology"));
        Assert.IsFalse(_resourceManager.HasUnseenWorkshop);
        _resourceManager.ActiveTab = "hand";

        // 3. Receive rewards (unlock it)
        _resourceManager.ReceiveRewards(refineMixology).Wait();

        // 4. Assert it's unlocked and flagged as unseen
        Assert.IsTrue(_resourceManager.UnlockedAbilities.Contains("Apprentice:Refine Mixology"));
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);

        // 5. Verify the refinement is in the discovered list (this is what the UI does)
        var discovered = _refinements!.ByKey
            .Where(r => _resourceManager.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")))
            .ToList();
        
        Assert.IsTrue(discovered.Any(d => d.Key.Name == "Refine Mixology"));
    }

    [TestMethod]
    public void LearningSameRefinementAbilityTwice_DoesNotReTriggerUnseen()
    {
        var apprentice = _cadences!.All.First(c => c.Name == "Apprentice");
        var refineMixology = apprentice.Abilities.First(a => a.Ability.Name == "Refine Mixology");

        _resourceManager!.ActiveTab = "hand";
        // First unlock
        _resourceManager!.ReceiveRewards(refineMixology).Wait();
        Assert.IsTrue(_resourceManager.HasUnseenWorkshop);
        _resourceManager.HasUnseenWorkshop = false;

        // Second unlock (e.g. from a different cadence if it had it, or just re-running)
        _resourceManager.ReceiveRewards(refineMixology).Wait();
        
        // Should NOT be unseen again if already known
        Assert.IsFalse(_resourceManager.HasUnseenWorkshop);
    }
}
