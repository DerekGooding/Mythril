using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_RefundCosts_CadenceUnlock_Works()
    {
        var character = _resourceManager!.Characters[0];
        var arcanist = _cadences!.All.First(c => c.Name == SandboxContent.Arcanist);
        var ability = arcanist.Abilities.First(a => a.Ability.Name == SandboxContent.MagicPocketI).Ability;
        var unlock = new CadenceUnlock(SandboxContent.Arcanist, ability, [new ItemQuantity(_items!.All.First(i => i.Name == SandboxContent.Gold), 100)], SandboxContent.Magic);

        foreach (var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        _resourceManager.StartQuest(unlock, character);

        Assert.HasCount(1, _resourceManager.ActiveQuests);
        var progress = _resourceManager.ActiveQuests[0];

        _resourceManager.CancelQuest(progress);
        Assert.IsEmpty(_resourceManager.ActiveQuests);
        foreach (var req in unlock.Requirements) Assert.AreEqual(req.Quantity, _resourceManager.Inventory.GetQuantity(req.Item));
    }

    [TestMethod]
    public void ResourceManager_RefundCosts_RefinementData_Works()
    {
        var character = _resourceManager!.Characters[0];
        var refData = _resourceManager.Refinements.GetRefinement(SandboxContent.RefineFire, SandboxContent.BasicGem)!.Value;

        // Need ability to start refinement
        var student = _cadences!.All.First(c => c.Name == SandboxContent.Student);
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.RefineFire);
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);

        _resourceManager.Inventory.Add(refData.InputItem, refData.Recipe.InputQuantity);
        _resourceManager.StartQuest(refData, character);

        Assert.HasCount(1, _resourceManager.ActiveQuests);
        var progress = _resourceManager.ActiveQuests[0];
        _resourceManager.CancelQuest(progress);

        Assert.AreEqual(refData.Recipe.InputQuantity, _resourceManager.Inventory.GetQuantity(refData.InputItem));
    }

    [TestMethod]
    public void ResourceManager_ToggleRecipeStar_Works()
    {
        _resourceManager!.ToggleRecipeStar("TestKey");
        Assert.Contains("TestKey", _resourceManager.StarredRecipes);
        _resourceManager.ToggleRecipeStar("TestKey");
        Assert.DoesNotContain("TestKey", _resourceManager.StarredRecipes);
    }

    [TestMethod]
    public void ResourceManager_HasAbility_Works()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        var ability = recruit.Abilities.First(a => a.Ability.Name == SandboxContent.AutoQuestI).Ability;

        Assert.IsFalse(_resourceManager.HasAbility(character, ability));

        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        Assert.IsTrue(_resourceManager.HasAbility(character, ability));
    }
}