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
        var item1 = new Item("Potion", "Heals 50 HP", ItemType.Consumable);
        var item2 = new Item("Ether", "Restores 20 MP", ItemType.Consumable);
        var spell1 = new Item("Fire", "Fire damage", ItemType.Spell);
        
        var inventory = Services.GetRequiredService<InventoryManager>();
        inventory.Clear();
        inventory.Add(item1, 5);
        inventory.Add(item2, 2);
        inventory.Add(spell1, 10);

        // Act
        var cut = RenderComponent<InventoryPanel>();
        
        // Ensure we show ALL items for the test count to match
        var allTab = cut.Find("[data-testid='inventory-tab-all']");
        allTab.Click();

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
    public void CharacterDisplay_RendersMultipleProgressBars_Correctly()
    {
        // Arrange
        var character = new Character("Hero");
        var quest1 = new Quest("Quest 1", "Desc");
        var quest2 = new Quest("Quest 2", "Desc");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        
        var progress1 = new QuestProgress(new QuestData(quest1, detail), "Desc", 10, character, 0);
        var progress2 = new QuestProgress(new QuestData(quest2, detail), "Desc", 10, character, 1);
        
        var progresses = new List<QuestProgress> { progress1, progress2 };

        // Act
        var cut = RenderComponent<CharacterDisplay>(parameters => parameters
            .Add(p => p.Character, character)
            .Add(p => p.QuestProgresses, progresses)
        );

        // Assert
        var container = cut.Find(".character-card-progress-container");
        var progressBars = container.QuerySelectorAll(".character-card-progress");
        
        Assert.AreEqual(2, progressBars.Length, "Should render two mini progress bars at the bottom.");
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

    [TestMethod]
    public void RefinementCard_RendersCorrectly()
    {
        // Arrange
        var ability = new CadenceAbility("Refine Fire", "Desc");
        var input = new Item("Basic Gem", "Desc", ItemType.Material);
        var output = new Item("Fire I", "Desc", ItemType.Spell);
        var recipe = new Recipe(1, output, 5);
        var refinement = new RefinementData(ability, input, recipe);

        // Act
        var cut = RenderComponent<RefinementCard>(parameters => parameters
            .Add(p => p.Refinement, refinement)
        );

        // Assert
        var text = cut.Find(".refinement-info").TextContent;
        Assert.IsTrue(text.Contains("1 Basic Gem"));
        Assert.IsTrue(text.Contains("5 Fire I"));
    }

    [TestMethod]
    public void ItemIcon_RendersSpellWithFallback()
    {
        // Arrange
        var item = new Item("Fire I", "Desc", ItemType.Spell);

        // Act
        var cut = RenderComponent<ItemIcon>(parameters => parameters.Add(p => p.Item, item));

        // Assert
        // Should have spell-glow class for spells
        var container = cut.Find("[data-testid='item-icon-fire-i']");
        Assert.IsTrue(container.ClassList.Contains("spell-glow"));
        
        // Initially shows sprite
        Assert.IsTrue(cut.FindAll(".item-sprite").Count > 0);

        // Simulate error
        var img = cut.Find(".item-sprite");
        img.TriggerEvent("onerror", EventArgs.Empty);

        // Should now show fallback
        var primitive = cut.Find(".spell-primitive");
        Assert.IsTrue(primitive.ClassList.Contains("fire"));
    }

    [TestMethod]
    public void ItemIcon_RendersMaterialWithFallback()
    {
        // Arrange
        var item = new Item("Iron Ore", "Desc", ItemType.Material);

        // Act
        var cut = RenderComponent<ItemIcon>(parameters => parameters.Add(p => p.Item, item));

        // Assert
        var container = cut.Find("[data-testid='item-icon-iron-ore']");
        Assert.IsFalse(container.ClassList.Contains("spell-glow"));

        // Simulate error
        var img = cut.Find(".item-sprite");
        img.TriggerEvent("onerror", EventArgs.Empty);

        // Should show initial
        var initial = cut.Find(".initial");
        Assert.AreEqual("I", initial.TextContent);
    }
}
