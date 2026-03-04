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
        ResourceManager.UnlockedAbilities.Add("Recruit:J-Str");
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);

        // Act - Increase Strength to 100
        InventoryManager.MagicCapacity = 1000;
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
        ResourceManager.UnlockedAbilities.Add("Recruit:J-Str");
        ResourceManager.UnlockedAbilities.Add("Student:J-Speed");
        
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);
        JunctionManager.AssignCadence(student, character, ResourceManager.UnlockedAbilities);

        // Act - Reach 100 in both STR and SPD
        InventoryManager.MagicCapacity = 2000;
        InventoryManager.Add(strMagic, 900);
        InventoryManager.Add(spdMagic, 900);
        
        JunctionManager.JunctionMagic(character, strStat, strMagic, ResourceManager.UnlockedAbilities);
        JunctionManager.JunctionMagic(character, spdStat, spdMagic, ResourceManager.UnlockedAbilities);

        ResourceManager.Tick(1.0);

        // Assert
        Assert.IsTrue(ResourceManager.UnlockedCadenceNames.Contains("Slayer"), "Slayer should be unlocked at 100 STR AND 100 SPD");
    }
}
