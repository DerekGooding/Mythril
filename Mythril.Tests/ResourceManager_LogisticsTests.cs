using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
        
        Assert.AreEqual(3, _resourceManager.ActiveQuests.Count);
        
        // Remove assignment
        _resourceManager.JunctionManager.Unassign(scholar, _resourceManager.UnlockedAbilities);
        
        // Manual call or via event
        _resourceManager.ReevaluateActiveQuests(character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsOnRequirementFailure()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.JStr);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        // Add 50 Fire I to inventory for Strength junction
        var fireI = _items!.All.First(i => i.Name == SandboxContent.FireI);
        _resourceManager.Inventory.Add(fireI, 50);
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat(SandboxContent.Strength, ""), fireI, _resourceManager.UnlockedAbilities);

        // Verify strength >= 15 (base 10 + 50/10 = 15)
        Assert.IsTrue(_resourceManager.JunctionManager.GetStatValue(character, SandboxContent.Strength) >= 15);

        // Create quest with Strength 16 requirement
        var quest = new Quest("Str Quest", "Requires 16 Str");
        var detail = new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { SandboxContent.Strength, 16 } });
        var questData = new QuestData(quest, detail);

        _resourceManager.StartQuest(questData, character);
        // StartQuest should fail because 15 < 16.
        // Wait, the test expects it to START then CANCEL.
        // So let's make the requirement 15 and then lower strength.
        
        var quest15 = new QuestData(quest, new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { SandboxContent.Strength, 15 } }));
        _resourceManager.StartQuest(quest15, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Quest should start with 15 Strength.");

        // Remove magic junction -> strength falls to 10
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat(SandboxContent.Strength, ""), new Item(), _resourceManager.UnlockedAbilities);
        Assert.AreEqual(10, _resourceManager.JunctionManager.GetStatValue(character, SandboxContent.Strength));

        // Reevaluate should cancel the quest
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Quest should be cancelled after Strength drop.");
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
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        // Remove assignment -> loses ability
        _resourceManager.JunctionManager.Unassign(student, _resourceManager.UnlockedAbilities);
        
        // Reevaluate should cancel the refinement
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }
}
