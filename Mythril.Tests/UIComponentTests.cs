using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mythril.Blazor.Components;
using Mythril.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class UIComponentTests : BunitTestBase
{
    [TestMethod]
    public void InventoryPanel_RendersCorrectly()
    {
        // Arrange
        var items = new List<ItemQuantity>
        {
            new ItemQuantity(new Item("Potion", "Heals 50 HP", ItemType.Consumable), 5),
            new ItemQuantity(new Item("Ether", "Restores 20 MP", ItemType.Consumable), 2)
        };
        var spells = new List<ItemQuantity>
        {
            new ItemQuantity(new Item("Fire", "Fire damage", ItemType.Spell), 10)
        };

        // Act
        var cut = RenderComponent<InventoryPanel>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.Spells, spells)
        );

        // Assert
        var itemElements = cut.FindComponents<InventoryItem>();
        Assert.AreEqual(3, itemElements.Count);
        
        var potion = itemElements.FirstOrDefault(i => i.Instance.Item.Item.Name == "Potion");
        Assert.IsNotNull(potion);
        Assert.AreEqual(5, potion.Instance.Item.Quantity);
    }

    [TestMethod]
    public void QuestProgressCard_RendersCorrectly()
    {
        // Arrange
        var character = new Character("Hero");
        var quest = new Quest("Test Quest", "Description");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var progress = new QuestProgress(new QuestData(quest, detail), "Description", 10, character);
        progress.SecondsElapsed = 5.0; // 50%

        // Act
        var cut = RenderComponent<QuestProgressCard>(parameters => parameters
            .Add(p => p.QuestProgress, progress)
        );

        // Assert
        // 1. Verify progress bar width
        var progressBar = cut.Find(".progress-bar");
        var style = progressBar.GetAttribute("style");
        Assert.IsNotNull(style);
        Assert.IsTrue(style.Contains("width: 50%"), $"Expected width: 50%, got {style}");

        // 2. Verify duration display
        var durationDisplay = cut.Find(".task-duration");
        Assert.AreEqual("10 s", durationDisplay.TextContent.Trim());
    }

    [TestMethod]
    public void QuestProgressCard_ZeroProgress_RendersCorrectly()
    {
        // Arrange
        var character = new Character("Hero");
        var quest = new Quest("Test Quest", "Description");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var progress = new QuestProgress(new QuestData(quest, detail), "Description", 10, character);
        progress.SecondsElapsed = 0;

        // Act
        var cut = RenderComponent<QuestProgressCard>(parameters => parameters
            .Add(p => p.QuestProgress, progress)
        );

        // Assert
        var progressBar = cut.Find(".progress-bar");
        var style = progressBar.GetAttribute("style");
        Assert.IsNotNull(style);
        Assert.IsTrue(style.Contains("width: 0%"), $"Expected width: 0%, got {style}");
    }

    [TestMethod]
    public void CharacterDisplay_RendersCorrectly()
    {
        // Arrange
        var character = new Character("Hero");
        
        // Act
        var cut = RenderComponent<CharacterDisplay>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.Accepts, (obj) => true)
        );

        // Assert
        var nameHeader = cut.Find("h4");
        Assert.AreEqual("Hero", nameHeader.TextContent.Trim());
        
        var cadenceInfo = cut.Find(".cadence-info");
        Assert.IsTrue(cadenceInfo.TextContent.Contains("Cadence: None"));
    }

    [TestMethod]
    public void CadencePanel_NoCadences_RendersWarning()
    {
        // Arrange
        var unlockedCadences = new List<Cadence>();
        
        // Act
        var cut = RenderComponent<CadencePanel>(parameters => parameters
            .Add(p => p.UnlockedCadences, unlockedCadences)
            .Add(p => p.UnlockedCadenceNamesCount, 0)
        );

        // Assert
        var noCadences = cut.Find(".no-cadences");
        Assert.IsTrue(noCadences.TextContent.Contains("No Cadences discovered"));
    }
}
