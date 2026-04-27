using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ProgressionTests : BunitTestBase
{
    [TestMethod]
    public void HiddenCadence_UnlocksAtStatThreshold()
    {
        // Arrange
        var character = ResourceManager.Characters[0];
        var stat = Stats.All.First(s => s.Name == SandboxContent.Strength);
        var magic = new Item("Strength Magic", "Desc", ItemType.Spell);

        // Setup character with J-Str
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        ResourceManager.UnlockCadence(recruit);
        ResourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.JStr);
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);

        // Act - Increase Strength to 100
        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magic, 900); // 10 (base) + 900/10 = 100
        JunctionManager.JunctionMagic(character, stat, magic, ResourceManager.UnlockedAbilities);

        // Trigger Tick to check thresholds
        ResourceManager.Tick(1.0);

        // Assert
        Assert.Contains(SandboxContent.Geologist, ResourceManager.UnlockedCadenceNames, "Geologist should be unlocked at 100 STR");
    }

    [TestMethod]
    public void MultiStatHiddenCadence_UnlocksCorrectly()
    {
        // Arrange
        var character = ResourceManager.Characters[0];
        var strStat = Stats.All.First(s => s.Name == SandboxContent.Strength);
        var spdStat = Stats.All.First(s => s.Name == SandboxContent.Speed);
        var strMagic = new Item("Str Magic", "Desc", ItemType.Spell);
        var spdMagic = new Item("Spd Magic", "Desc", ItemType.Spell);

        // Setup character with J-Str and J-Speed
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Student);
        ResourceManager.UnlockCadence(recruit);
        ResourceManager.UnlockCadence(student);
        ResourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.JStr);
        ResourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.JSpd);

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
        Assert.Contains(SandboxContent.Slayer, ResourceManager.UnlockedCadenceNames, "Slayer should be unlocked at 100 STR AND 100 SPD");
    }

    [TestMethod]
    public void TideCaller_UnlocksAtSpeedThreshold()
    {
        var character = ResourceManager.Characters[0];
        var spdStat = Stats.All.First(s => s.Name == SandboxContent.Speed);
        var spdMagic = new Item("Spd Magic", "Desc", ItemType.Spell);

        var student = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Student);
        ResourceManager.UnlockCadence(student);
        ResourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.JSpd);
        JunctionManager.AssignCadence(student, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(spdMagic, 500); // 10 (base) + 500/10 = 60
        JunctionManager.JunctionMagic(character, spdStat, spdMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.Contains(SandboxContent.TideCaller, ResourceManager.UnlockedCadenceNames);
    }

    [TestMethod]
    public void Sentinel_UnlocksAtVitalityThreshold()
    {
        var character = ResourceManager.Characters[0];
        var vitStat = Stats.All.First(s => s.Name == SandboxContent.Vitality);
        var vitMagic = new Item("Vit Magic", "Desc", ItemType.Spell);

        var weaver = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Weaver);
        ResourceManager.UnlockCadence(weaver);
        ResourceManager.UnlockAbility(SandboxContent.Weaver, SandboxContent.JVit);
        JunctionManager.AssignCadence(weaver, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(vitMagic, 500); // 60 VIT
        JunctionManager.JunctionMagic(character, vitStat, vitMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.Contains(SandboxContent.Sentinel, ResourceManager.UnlockedCadenceNames);
    }

    [TestMethod]
    public void Scholar_UnlocksAtMagicThreshold()
    {
        var character = ResourceManager.Characters[0];
        var magStat = Stats.All.First(s => s.Name == SandboxContent.Magic);
        var magMagic = new Item("Mag Magic", "Desc", ItemType.Spell);

        var arcanist = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Arcanist);
        ResourceManager.UnlockCadence(arcanist);
        ResourceManager.UnlockAbility(SandboxContent.Arcanist, SandboxContent.JMag);
        JunctionManager.AssignCadence(arcanist, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magMagic, 900); // 100 MAG
        JunctionManager.JunctionMagic(character, magStat, magMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);
        Assert.Contains(SandboxContent.Scholar, ResourceManager.UnlockedCadenceNames);
    }

    [TestMethod]
    public void MagicPocket_IncreasesCapacity()
    {
        Assert.AreEqual(30, InventoryManager.MagicCapacity);

        ResourceManager.UnlockAbility(SandboxContent.Arcanist, SandboxContent.MagicPocketI);

        Assert.AreEqual(60, InventoryManager.MagicCapacity);

        ResourceManager.UnlockAbility(SandboxContent.Sentinel, SandboxContent.MagicPocketII);

        Assert.AreEqual(100, InventoryManager.MagicCapacity);
    }

    [TestMethod]
    public void LocationDiscovery_Works()
    {
        ResourceManager.UpdateUsableLocations();
        // Greenwood Forest is not in sandbox, use Forest
        var forest = ContentHost.GetContent<Locations>().All.First(l => l.Name == "Forest");
        var reqQuestName = forest.RequiredQuest;
        Assert.IsNotNull(reqQuestName, "RequiredQuest should not be null for Forest");

        Assert.DoesNotContain("Forest", ResourceManager.UnlockedLocationNames, "Forest should not be unlocked initially");

        // Complete the required quest
        var quests = ContentHost.GetContent<Quests>();
        var quest = quests.All.First(q => q.Name == reqQuestName);
        var questDetails = ContentHost.GetContent<QuestDetails>();
        var questData = new QuestData(quest, questDetails[quest]);

        ResourceManager.ReceiveRewards(questData).Wait(TestContext.CancellationToken);

        // Check if quest is completed in state
        Assert.Contains(reqQuestName, GameStore.State.CompletedQuests, $"Quest {reqQuestName} should be completed in state");

        ResourceManager.UpdateUsableLocations();

        // Diagnostic check of UsableLocations
        var isUsable = ResourceManager.UsableLocations.Any(l => l.Name == "Forest");
        Assert.IsTrue(isUsable, "Forest should be in UsableLocations after completion");

        Assert.Contains("Forest", ResourceManager.UnlockedLocationNames, "Forest should be in UnlockedLocationNames");
    }

    [TestMethod]
    public void CheckHiddenCadences_TriggersUnlock()
    {
        var character = ResourceManager.Characters[0];
        var magStat = Stats.All.First(s => s.Name == SandboxContent.Magic);
        var magMagic = new Item("Mag Magic", "Desc", ItemType.Spell);

        var arcanist = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Arcanist);
        ResourceManager.UnlockCadence(arcanist);
        ResourceManager.UnlockAbility(SandboxContent.Arcanist, SandboxContent.JMag);
        JunctionManager.AssignCadence(arcanist, character, ResourceManager.UnlockedAbilities);

        GameStore.Dispatch(new SetMagicCapacityAction(1000));
        InventoryManager.Add(magMagic, 900);
        JunctionManager.JunctionMagic(character, magStat, magMagic, ResourceManager.UnlockedAbilities);

        // Act
        ResourceManager.CheckHiddenCadences();

        // Assert
        Assert.Contains(SandboxContent.Scholar, ResourceManager.UnlockedCadenceNames);
    }

    [TestMethod]
    public void IsInProgress_QuestData_Works()
    {
        var character = ResourceManager.Characters[0];
        var quest = ContentHost.GetContent<Quests>().All[0];
        var data = new QuestData(quest, ContentHost.GetContent<QuestDetails>()[quest]);

        Assert.IsFalse(ResourceManager.IsInProgress(data));

        ResourceManager.StartQuest(data, character);
        Assert.IsTrue(ResourceManager.IsInProgress(data));
    }

    public TestContext TestContext { get; set; }
}