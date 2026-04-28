using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JunctionManagementTests : ResourceManagerTestBase
{
    [TestMethod]
    public void JunctionManager_AssignCadence_SupportsMultipleCadences()
    {
        var character = _resourceManager!.Characters[0];
        var cadence1 = _cadences!.All[0];
        var cadence2 = _cadences!.All[1];

        _resourceManager.JunctionManager.AssignCadence(cadence1, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(cadence2, character, _resourceManager.UnlockedAbilities);

        var assigned = _resourceManager.JunctionManager.CurrentlyAssigned(character).ToList();
        Assert.HasCount(2, assigned);
        Assert.Contains(cadence1, assigned);
        Assert.Contains(cadence2, assigned);
    }

    [TestMethod]
    public void JunctionManager_AssignCadence_MaintainsExclusivity()
    {
        var character1 = _resourceManager!.Characters[0];
        var character2 = _resourceManager!.Characters[1];
        var cadence = _cadences!.All[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character1, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(cadence, character2, _resourceManager.UnlockedAbilities);

        Assert.IsFalse(_resourceManager.JunctionManager.CurrentlyAssigned(character1).Contains(cadence));
        Assert.IsTrue(_resourceManager.JunctionManager.CurrentlyAssigned(character2).Contains(cadence));
    }

    [TestMethod]
    public void JunctionManager_AssignCadence_Works()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(cadence, _resourceManager.JunctionManager.CurrentlyAssigned(character).First());
    }

    [TestMethod]
    public void JunctionManager_CurrentlyAssigned_ReturnsCorrectCadences()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        var assigned = _resourceManager.JunctionManager.CurrentlyAssigned(character);

        Assert.IsTrue(assigned.Contains(cadence));
    }

    [TestMethod]
    public void JunctionManager_Unassign_ClearsAssignments()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All[0];

        _resourceManager.JunctionManager.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.Unassign(cadence, _resourceManager.UnlockedAbilities);

        Assert.IsFalse(_resourceManager.JunctionManager.CurrentlyAssigned(character).Any());
    }

    [TestMethod]
    public void JunctionManager_Unassign_UnassignedCadence()
    {
        var cadence = _cadences!.All[0];
        // Should not throw
        _resourceManager!.JunctionManager.Unassign(cadence, _resourceManager!.UnlockedAbilities);
    }

    [TestMethod]
    public void JunctionManager_Unassign_InvalidatesJunctions()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        var strStat = ContentHost.GetContent<Stats>().All.First(s => s.Name == "Strength");
        var fire = ContentHost.GetContent<Items>().All.First(i => i.Name == "Fire I");

        _resourceManager.UnlockAbility("Recruit", "J-Str");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        _resourceManager.JunctionManager.JunctionMagic(character, strStat, fire, _resourceManager.UnlockedAbilities);
        Assert.HasCount(1, _resourceManager.JunctionManager.Junctions);

        _resourceManager.JunctionManager.Unassign(recruit, _resourceManager.UnlockedAbilities);
        Assert.IsEmpty(_resourceManager.JunctionManager.Junctions, "Junction should be removed when ability is lost.");
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_DoesNothingWithoutAbility()
    {
        var character = _resourceManager!.Characters[0];
        var strStat = ContentHost.GetContent<Stats>().All.First(s => s.Name == "Strength");
        var fire = ContentHost.GetContent<Items>().All.First(i => i.Name == "Fire I");

        _resourceManager!.JunctionManager.JunctionMagic(character, strStat, fire, _resourceManager!.UnlockedAbilities);
        Assert.IsEmpty(_resourceManager.JunctionManager.Junctions);
    }

    [TestMethod]
    public void JunctionManager_Unassign_OnlyClearsInvalidJunctions()
    {
        var character = _resourceManager!.Characters[0];
        var abilities = ContentHost.GetContent<CadenceAbilities>();
        var stats = ContentHost.GetContent<Stats>();
        var abilityJStr = abilities.All.First(a => a.Name == SandboxContent.JStr);
        var cadence1 = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var cadence2 = new Cadence("Extra Cadence", "Desc", [new CadenceUnlock("Extra Cadence", abilityJStr, [])]);
        _cadences.Load(_cadences.All.Concat([cadence2]));
        var strengthStat = stats.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager!.UnlockAbility("Extra Cadence", SandboxContent.JStr);
        _resourceManager.UnlockAbility(cadence1.Name, SandboxContent.JStr);
        _resourceManager.JunctionManager.AssignCadence(cadence1, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(cadence2, character, _resourceManager.UnlockedAbilities);

        _resourceManager.JunctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.HasCount(1, _resourceManager.JunctionManager.Junctions);

        // Unassign one cadence with the ability, but the other still has it
        _resourceManager.JunctionManager.Unassign(cadence1, _resourceManager.UnlockedAbilities);
        Assert.HasCount(1, _resourceManager.JunctionManager.Junctions);

        // Unassign the last cadence with the ability, junction should be gone
        _resourceManager.JunctionManager.Unassign(cadence2, _resourceManager.UnlockedAbilities);
        Assert.IsEmpty(_resourceManager.JunctionManager.Junctions);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var stats = ContentHost.GetContent<Stats>();
        var cadenceWithoutJStr = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var cadenceWithJStr = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var strengthStat = stats.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager!.UnlockAbility(cadenceWithJStr.Name, SandboxContent.JStr);
        _resourceManager.JunctionManager.AssignCadence(cadenceWithoutJStr, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(cadenceWithJStr, character, _resourceManager.UnlockedAbilities);

        _resourceManager.JunctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.HasCount(1, _resourceManager.JunctionManager.Junctions);
    }

    [TestMethod]
    public void ResourceManager_CanAutoQuest_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var cadenceWithoutAuto = _cadences!.All.First(c => c.Name == SandboxContent.Arcanist);
        var cadenceWithAuto = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.AutoQuestI));

        _resourceManager!.UnlockAbility(cadenceWithAuto.Name, SandboxContent.AutoQuestI);

        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        _resourceManager.JunctionManager.AssignCadence(cadenceWithoutAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        _resourceManager.JunctionManager.AssignCadence(cadenceWithAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsTrue(_resourceManager.CanAutoQuest(character));
    }

    [TestMethod]
    public void ResourceManager_UnlockedAbilities_ArePerCadence()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);

        _resourceManager!.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);

        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        Assert.IsTrue(_resourceManager.CanAutoQuest(character));

        _resourceManager.JunctionManager.Unassign(recruit, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(apprentice, character, _resourceManager.UnlockedAbilities);

        Assert.IsFalse(_resourceManager.CanAutoQuest(character));
    }
}