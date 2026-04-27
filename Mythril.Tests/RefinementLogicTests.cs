using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class RefinementLogicTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ItemRefinements_ByKey_IsPopulated()
    {
        Assert.IsNotNull(_resourceManager!.Refinements);
        Assert.IsNotEmpty(_resourceManager.Refinements.ByKey, "Refinements should be loaded.");

        var hasFire = _resourceManager.Refinements.ByKey.Any(r => r.Key.Name == "Refine Fire");
        Assert.IsTrue(hasFire, "Refine Fire should be in the refinements dictionary.");
    }

    [TestMethod]
    public void WorkshopDiscovery_Logic_Works()
    {
        // Simulate unlocking "Refine Fire" on "Student"
        _resourceManager!.UnlockAbility("Student", "Refine Fire");

        // Logic from Workshop.razor
        var discoveredRefinements = _resourceManager.Refinements.ByKey
            .Where(r => _resourceManager.UnlockedAbilities.Any(ua => ua.EndsWith($":{r.Key.Name}")))
            .ToList();

        Assert.Contains(r => r.Key.Name == "Refine Fire", discoveredRefinements, "Refine Fire should be discovered.");
    }

    [TestMethod]
    public void WorkshopAbility_Logic_Works()
    {
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        var ability = student.Abilities.First(a => a.Ability.Name == "Refine Fire").Ability;
        var character = _resourceManager!.Characters[0];

        // Unlock it
        _resourceManager.UnlockAbility("Student", "Refine Fire");

        // Check if anyone has it (should be false as not assigned)
        var anyoneHasAbility = _resourceManager.Characters.Any(c => _resourceManager.HasAbility(c, ability));
        Assert.IsFalse(anyoneHasAbility, "Should be false because cadence is not assigned.");

        // Assign cadence
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        // Check again
        anyoneHasAbility = _resourceManager.Characters.Any(c => _resourceManager.HasAbility(c, ability));
        Assert.IsTrue(anyoneHasAbility, "Should be true because Student is assigned and ability is unlocked.");
    }

    [TestMethod]
    public async Task RefinementExecution_AddsItemsToInventory()
    {
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        var ability = student.Abilities.First(a => a.Ability.Name == "Refine Fire").Ability;
        var basicGem = ContentHost.GetContent<Items>().All.First(i => i.Name == "Basic Gem");
        var fireI = ContentHost.GetContent<Items>().All.First(i => i.Name == "Fire I");
        var recipe = _resourceManager!.Refinements.ByKey[ability].Recipes[basicGem];

        var refinementData = new RefinementData(ability, basicGem, recipe, "Magic");
        var character = _resourceManager!.Characters[0];

        // Unlock and Assign cadence so character has ability
        _resourceManager.UnlockAbility("Student", "Refine Fire");
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        // Ensure inventory is empty
        _resourceManager.Inventory.Clear();
        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(fireI));

        // Start quest (Pay costs)
        _resourceManager.Inventory.Add(basicGem, 1);
        _resourceManager.StartQuest(refinementData, character);
        Assert.AreEqual(0, _resourceManager.Inventory.GetQuantity(basicGem), "Basic Gem should have been consumed.");

        // Get progress and verify name
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(refinementData.Name, progress.Name);
        Assert.IsFalse(string.IsNullOrEmpty(progress.Name), "Progress name should not be empty for refinements.");

        // Finish quest (Receive rewards)
        await _resourceManager.ReceiveRewards(progress.Item);

        Assert.AreEqual(recipe.OutputQuantity, _resourceManager.Inventory.GetQuantity(fireI), "Should have received fire magic from refinement.");
    }
}