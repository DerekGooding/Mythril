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
        var scholar = _cadences!.All.First(c => c.Name == "Scholar");
        _resourceManager.UnlockCadence(scholar);
        _resourceManager.UnlockAbility("Scholar", "Logistics II");
        _resourceManager.JunctionManager.AssignCadence(scholar, character, _resourceManager.UnlockedAbilities);
        
        // Initial limit is 1. With Logistics II, it should be 3.
        Assert.AreEqual(3, _resourceManager.GetTaskLimit(character));
        
        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        var q2 = new QuestData(_quests.All.First(q => q.Name == "Hunt Bats"), _questDetails[_quests.All.First(q => q.Name == "Hunt Bats")]);
        var q3 = new QuestData(_quests.All.First(q => q.Name == "Hunt Spiders"), _questDetails[_quests.All.First(q => q.Name == "Hunt Spiders")]);
        
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
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility("Recruit", "J-Str");
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);

        // Add 100 Magic I to inventory for Strength junction
        var magicI = new Item("Magic I", "Magic", ItemType.Spell);
        _resourceManager.Inventory.Add(magicI, 100);
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat("Strength", ""), magicI, _resourceManager.UnlockedAbilities);

        // Verify strength > 10 (base 10 + 100/10 = 20)
        Assert.IsTrue(_resourceManager.JunctionManager.GetStatValue(character, "Strength") >= 20);

        // Create quest with Strength 15 requirement
        var quest = new Quest("Str Quest", "Requires 15 Str");
        var detail = new QuestDetail(10, [], [], QuestType.Recurring, RequiredStats: new Dictionary<string, int> { { "Strength", 15 } });
        var questData = new QuestData(quest, detail);

        _resourceManager.StartQuest(questData, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);

        // Remove magic junction -> strength falls to 10
        _resourceManager.JunctionManager.JunctionMagic(character, new Stat("Strength", ""), new Item(), _resourceManager.UnlockedAbilities);
        Assert.AreEqual(10, _resourceManager.JunctionManager.GetStatValue(character, "Strength"));

        // Reevaluate should cancel the quest
        _resourceManager.ReevaluateActiveQuests(character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count);
    }

    [TestMethod]
    public void ReevaluateActiveQuests_CancelsRefinementOnAbilityLoss()
    {
        var character = _resourceManager!.Characters[0];
        var refData = _resourceManager.Refinements.GetRefinement("Refine Fire", "Basic Gem")!.Value;
        
        // Need ability to start refinement
        var student = _cadences!.All.First(c => c.Name == "Student");
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockAbility("Student", "Refine Fire");
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
