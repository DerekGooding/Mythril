using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Mythril.Blazor.Components;
using Mythril.Data;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class JunctionOverhaulTests : BunitTestBase
{
    [TestMethod]
    public void StatValue_EnforcesCeiling()
    {
        // Arrange
        var character = new Character("Hero");
        var stat = Stats.All.First(s => s.Name == "Strength");
        var magic = new Item("Ultimate Magic", "OP", ItemType.Spell);
        
        // Increase capacity to allow reaching 255
        GameStore.Dispatch(new SetMagicCapacityAction(10000));
        InventoryManager.Add(magic, 10000);
        
        // Setup junction
        ResourceManager.UnlockedAbilities.Add("Warrior:J-Str");
        var warrior = new Cadence("Warrior", "Desc", [new CadenceUnlock("Warrior", new CadenceAbility("J-Str", "Desc"), [])]);
        JunctionManager.AssignCadence(warrior, character, ResourceManager.UnlockedAbilities);
        JunctionManager.JunctionMagic(character, stat, magic, ResourceManager.UnlockedAbilities);

        // Act
        int val = JunctionManager.GetStatValue(character, "Strength");

        // Assert
        Assert.AreEqual(255, val, "Stat should be capped at 255");
    }

    [TestMethod]
    public void CharacterDisplay_RemovalMode_TogglesCorrectly()
    {
        // Arrange
        var character = new Character("Hero");
        var cadence = new Cadence("Warrior", "Desc", []);
        JunctionManager.AssignCadence(cadence, character, []);

        var cut = RenderComponent<CharacterDisplay>(p => p
            .Add(cp => cp.Character, character)
        );

        // Act - Click removal button (link_off icon)
        var removalBtn = cut.Find("button[title='Remove Junctions']");
        removalBtn.Click();

        // Assert
        Assert.IsTrue(removalBtn.ClassList.Contains("btn-danger"));
        
        // Act - Toggle menu should turn off removal
        var menuBtn = cut.Find("button[title='Junction Menu']");
        menuBtn.Click();
        
        // Assert
        Assert.IsFalse(removalBtn.ClassList.Contains("btn-danger"));
    }

    [TestMethod]
    public void CharacterDisplay_ShowsDeltaPreview()
    {
        // Arrange
        var character = new Character("Hero");
        var stat = Stats.All.First(s => s.Name == "Strength");
        var magic = new Item("Fire", "Burn", ItemType.Spell);
        
        GameStore.Dispatch(new SetMagicCapacityAction(100));
        InventoryManager.Add(magic, 50); // 50 / 10 = +5 Strength

        // Ensure CanJunction returns true
        var cadence = new Cadence("Warrior", "Desc", [new CadenceUnlock("Warrior", new CadenceAbility("J-Str", "Desc"), [])]);
        JunctionManager.AssignCadence(cadence, character, ["Warrior:J-Str"]);
        ResourceManager.UnlockedAbilities.Add("Warrior:J-Str");

        // Mock dragging the item
        DragDropService.Data = magic;
        DragDropService.SetHoveredTarget(character, stat);

        var cut = RenderComponent<CharacterDisplay>(p => p
            .Add(cp => cp.Character, character)
        );

        // Act & Assert
        var delta = cut.Find(".stat-delta");
        Assert.IsTrue(delta.TextContent.Contains("↑5"));
        Assert.IsTrue(delta.ClassList.Contains("text-success"));
    }
}
