using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class ProgressionTests : BunitTestBase
{
    [TestMethod]
    public void HiddenCadence_UnlocksAtStatThreshold()
    {
        // Arrange
        var character = ResourceManager.Characters[0];
        var stat = Stats.All.First(s => s.Name == "Strength");
        var magic = new Item("Strength Magic", "Desc", ItemType.Spell);
        
        // Setup character with J-Str
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Recruit");
        ResourceManager.UnlockCadence(recruit);
        ResourceManager.UnlockAbility("Recruit", "J-Str");
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);

        // Act - Increase Strength to 100
        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magic, 900); // 10 (base) + 900/10 = 100
        JunctionManager.JunctionMagic(character, stat, magic, ResourceManager.UnlockedAbilities);

        // Trigger Tick to check thresholds
        ResourceManager.Tick(1.0);

        // Assert
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Geologist"), "Geologist should be unlocked at 100 STR");
    }

    [TestMethod]
    public void MultiStatHiddenCadence_UnlocksCorrectly()
    {
        // Arrange
        var character = ResourceManager.Characters[0];
        var strStat = Stats.All.First(s => s.Name == "Strength");
        var spdStat = Stats.All.First(s => s.Name == "Speed");
        var strMagic = new Item("Str Magic", "Desc", ItemType.Spell);
        var spdMagic = new Item("Spd Magic", "Desc", ItemType.Spell);
        
        // Setup character with J-Str and J-Speed
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Recruit");
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        ResourceManager.UnlockCadence(recruit);
        ResourceManager.UnlockCadence(student);
        ResourceManager.UnlockAbility("Recruit", "J-Str");
        ResourceManager.UnlockAbility("Student", "J-Speed");
        
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);
        JunctionManager.AssignCadence(student, character, ResourceManager.UnlockedAbilities);

        // Act - Reach 100 in both STR and SPD
        GameStore.Dispatch(new SetMagicCapacityAction(2000));
        InventoryManager.Add(strMagic, 900);
        InventoryManager.Add(spdMagic, 900);
        
        JunctionManager.JunctionMagic(character, strStat, strMagic, ResourceManager.UnlockedAbilities);
        JunctionManager.JunctionMagic(character, spdStat, spdMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);

        // Assert
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Slayer"), "Slayer should be unlocked at 100 STR AND 100 SPD");
    }

    [TestMethod]
    public void TideCaller_UnlocksAtSpeedThreshold()
    {
        var character = ResourceManager.Characters[0];
        var spdStat = Stats.All.First(s => s.Name == "Speed");
        var spdMagic = new Item("Spd Magic", "Desc", ItemType.Spell);
        
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Student");
        ResourceManager.UnlockCadence(student);
        ResourceManager.UnlockAbility("Student", "J-Speed");
        JunctionManager.AssignCadence(student, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(spdMagic, 500); // 10 (base) + 500/10 = 60
        JunctionManager.JunctionMagic(character, spdStat, spdMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Tide-Caller"));
    }

    [TestMethod]
    public void Sentinel_UnlocksAtVitalityThreshold()
    {
        var character = ResourceManager.Characters[0];
        var vitStat = Stats.All.First(s => s.Name == "Vitality");
        var vitMagic = new Item("Vit Magic", "Desc", ItemType.Spell);
        
        var weaver = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Mythril Weaver");
        ResourceManager.UnlockCadence(weaver);
        ResourceManager.UnlockAbility("Mythril Weaver", "J-Vit");
        JunctionManager.AssignCadence(weaver, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(vitMagic, 500); // 60 VIT
        JunctionManager.JunctionMagic(character, vitStat, vitMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("The Sentinel"));
    }

    [TestMethod]
    public void Scholar_UnlocksAtMagicThreshold()
    {
        var character = ResourceManager.Characters[0];
        var magStat = Stats.All.First(s => s.Name == "Magic");
        var magMagic = new Item("Mag Magic", "Desc", ItemType.Spell);
        
        var arcanist = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Arcanist");
        ResourceManager.UnlockCadence(arcanist);
        ResourceManager.UnlockAbility("Arcanist", "J-Magic");
        JunctionManager.AssignCadence(arcanist, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magMagic, 900); // 100 MAG
        JunctionManager.JunctionMagic(character, magStat, magMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Scholar"));
    }

    [TestMethod]
    public void MagicPocket_IncreasesCapacity()
    {
        Assert.AreEqual(30, InventoryManager.MagicCapacity);

        ResourceManager.UnlockAbility("Arcanist", "Magic Pocket I");

        Assert.AreEqual(60, InventoryManager.MagicCapacity);

        ResourceManager.UnlockAbility("The Sentinel", "Magic Pocket II");

        Assert.AreEqual(100, InventoryManager.MagicCapacity);
    }

    [TestMethod]
    public void LocationDiscovery_Works()
    {
        ResourceManager.UpdateUsableLocations();
        var forest = ContentHost.GetContent<Locations>().All.First(l => l.Name == "Greenwood Forest");
        var reqQuestName = forest.RequiredQuest;
        Assert.IsNotNull(reqQuestName, "RequiredQuest should not be null for Greenwood Forest");

        Assert.IsFalse(ResourceManager.UnlockedLocationNames.Contains("Greenwood Forest"), "Forest should not be unlocked initially");

        // Complete the required quest
        var quests = ContentHost.GetContent<Quests>();
        var quest = quests.All.First(q => q.Name == reqQuestName);
        var questDetails = ContentHost.GetContent<QuestDetails>();
        var questData = new QuestData(quest, questDetails[quest]);
        
        ResourceManager.ReceiveRewards(questData).Wait();

        // Check if quest is completed in state
        Assert.IsTrue(GameStore.State.CompletedQuests.Contains(reqQuestName), $"Quest {reqQuestName} should be completed in state");

        ResourceManager.UpdateUsableLocations();
        
        // Diagnostic check of UsableLocations
        var isUsable = ResourceManager.UsableLocations.Any(l => l.Name == "Greenwood Forest");
        Assert.IsTrue(isUsable, "Greenwood Forest should be in UsableLocations after completion");

        Assert.IsTrue(ResourceManager.UnlockedLocationNames.Contains("Greenwood Forest"), "Greenwood Forest should be in UnlockedLocationNames");
    }

    [TestMethod]
    public void CheckHiddenCadences_TriggersUnlock()
    {
        var character = ResourceManager.Characters[0];
        var magStat = Stats.All.First(s => s.Name == "Magic");
        var magMagic = new Item("Mag Magic", "Desc", ItemType.Spell);

        var arcanist = ContentHost.GetContent<Cadences>().All.First(c => c.Name == "Arcanist");
        ResourceManager.UnlockCadence(arcanist);
        ResourceManager.UnlockAbility("Arcanist", "J-Magic");
        JunctionManager.AssignCadence(arcanist, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magMagic, 900);
        JunctionManager.JunctionMagic(character, magStat, magMagic, ResourceManager.UnlockedAbilities);

        // Act
        ResourceManager.CheckHiddenCadences();

        // Assert
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Scholar"));
    }

    [TestMethod]
    public void IsInProgress_QuestData_Works()
    {
        var character = ResourceManager.Characters[0];
        var quest = ContentHost.GetContent<Quests>().All.First();
        var data = new QuestData(quest, ContentHost.GetContent<QuestDetails>()[quest]);

        Assert.IsFalse(ResourceManager.IsInProgress(data));

        ResourceManager.StartQuest(data, character);
        Assert.IsTrue(ResourceManager.IsInProgress(data));
    }
}
