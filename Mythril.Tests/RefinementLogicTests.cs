using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class RefinementLogicTests
{
    private ResourceManager? _resourceManager;
    private ItemRefinements? _refinements;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _refinements = ContentHost.GetContent<ItemRefinements>();
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), ContentHost.GetContent<Cadences>());
        
        _resourceManager = new ResourceManager(
            ContentHost.GetContent<Items>(), 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            ContentHost.GetContent<Cadences>(), 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            _refinements);
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void ItemRefinements_ByKey_IsPopulated()
    {
        Assert.IsNotNull(_refinements);
        Assert.IsTrue(_refinements.ByKey.Count > 0, "Refinements should be loaded.");
        
        var hasFire = _refinements.ByKey.Any(r => r.Key.Name == "Refine Fire");
        Assert.IsTrue(hasFire, "Refine Fire should be in the refinements dictionary.");
    }

    [TestMethod]
    public void WorkshopDiscovery_Logic_Works()
    {
        // Simulate unlocking "Refine Fire" on "Student"
        _resourceManager!.UnlockedAbilities.Add("Student:Refine Fire");

        // Logic from Workshop.razor
        var discoveredRefinements = _refinements!.ByKey
            .Where(r => _resourceManager.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")))
            .ToList();

        Assert.IsTrue(discoveredRefinements.Any(r => r.Key.Name == "Refine Fire"), "Refine Fire should be discovered.");
    }

    [TestMethod]
    public void WorkshopAbility_Logic_Works()
    {
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        var ability = student.Abilities.First(a => a.Ability.Name == "Refine Fire").Ability;
        var character = _resourceManager!.Characters[0];

        // Unlock it
        _resourceManager.UnlockedAbilities.Add("Student:Refine Fire");
        
        // Check if anyone has it (should be false as not assigned)
        var anyoneHasAbility = _resourceManager.Characters.Any(c => _resourceManager.HasAbility(c, ability));
        Assert.IsFalse(anyoneHasAbility, "Should be false because cadence is not assigned.");

        // Assign cadence
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        // Check again
        anyoneHasAbility = _resourceManager.Characters.Any(c => _resourceManager.HasAbility(c, ability));
        Assert.IsTrue(anyoneHasAbility, "Should be true because Student is assigned and ability is unlocked.");
    }
}
