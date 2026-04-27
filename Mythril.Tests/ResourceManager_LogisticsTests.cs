using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManager_LogisticsTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_ReevaluateActiveQuests_Works()
    {
        var character = _resourceManager!.Characters[0];
        var scholar = _cadences!.All.First(c => c.Name == SandboxContent.Scholar);
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility(SandboxContent.Scholar, SandboxContent.LogisticsII);
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);

        // Initial limit is 1. With Logistics II (Effect Logistics, 2), it should be 1 + 2 = 3.
        Assert.AreEqual(3, _resourceManager.GetTaskLimit(character));

        var q1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.HuntGoblins), _questDetails![_quests.All.First(q => q.Name == SandboxContent.HuntGoblins)]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.HuntBats), _questDetails[_quests.All.First(q => q.Name == SandboxContent.HuntBats)]);
        var q3 = new QuestData(_quests.All.First(q => q.Name == SandboxContent.HuntSpiders), _questDetails[_quests.All.First(q => q.Name == SandboxContent.HuntSpiders)]);

        _resourceManager.StartQuest(q1, character);
        _resourceManager.StartQuest(q2, character);
        _resourceManager.StartQuest(q3, character);

        Assert.HasCount(3, _resourceManager.ActiveQuests);

        // Remove assignment
        _resourceManager.JunctionManager.Unassign(scholar, _resourceManager.UnlockedAbilities);

        // Manual call or via event
        _resourceManager.ReevaluateActiveQuests(character);

        Assert.HasCount(1, _resourceManager.ActiveQuests);
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsOnRequirementFailure()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.JStr);
        _resourceManager.UnlockAbility(SandboxContent.Arcanist, SandboxContent.MagicPocketI); // Increase capacity to 60
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        // Add 50 Fire I to inventory for Strength junction
        var fireI = _items!.All.First(i => i.Name == SandboxContent.FireI);
        _resourceManager.Inventory.Add(fireI, 50);
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat(SandboxContent.Strength, ""), fireI, _resourceManager.UnlockedAbilities);

        // Verify strength == 16 (base 10 + 1 boost + 50/10 = 16)
        Assert.AreEqual(16, _resourceManager.JunctionManager.GetStatValue(character, SandboxContent.Strength));

        // Create quest with Strength 17 requirement
        var quest = new Quest("Str Quest", "Requires 17 Str");
        var detail = new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { SandboxContent.Strength, 17 } });
        var questData = new QuestData(quest, detail);

        _resourceManager.StartQuest(questData, character);
        // StartQuest should fail because 16 < 17.

        var quest16 = new QuestData(quest, new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { SandboxContent.Strength, 16 } }));
        _resourceManager.StartQuest(quest16, character);
        Assert.HasCount(1, _resourceManager.ActiveQuests, "Quest should start with 16 Strength.");

        // Remove magic junction -> strength falls to 11 (base 10 + 1 boost)
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat(SandboxContent.Strength, ""), new Item(), _resourceManager.UnlockedAbilities);
        Assert.AreEqual(11, _resourceManager.JunctionManager.GetStatValue(character, SandboxContent.Strength));

        // Reevaluate should cancel the quest
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.IsEmpty(_resourceManager.ActiveQuests, "Quest should be cancelled after Strength drop.");
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsRefinementOnAbilityLoss()
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

        // Remove assignment -> loses ability
        _resourceManager.JunctionManager.Unassign(student, _resourceManager.UnlockedAbilities);

        // Reevaluate should cancel the refinement
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.IsEmpty(_resourceManager.ActiveQuests);
    }
}