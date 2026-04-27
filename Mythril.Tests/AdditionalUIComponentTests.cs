using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Mythril.Blazor;
using Mythril.Blazor.Components;
using Mythril.Blazor.Layout;
using Mythril.Blazor.Pages;
using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class AdditionalUIComponentTests : BunitTestBase
{
    [TestMethod]
    public void AbilityUnlockCard_RendersCorrectly()
    {
        var ability = new CadenceAbility("Test Ability", "Description");
        var unlock = new CadenceUnlock("Test Cadence", ability, []);
        var cut = RenderComponent<AbilityUnlockCard>(parameters => parameters.Add(p => p.Unlock, unlock));
        Assert.Contains("Test Ability", cut.Markup);
    }

    [TestMethod]
    public void CadenceDragExpander_RendersCorrectly()
    {
        var cadence = new Cadence("Test Cadence", "Description", []);
        var cut = RenderComponent<CadenceDragExpander>(parameters => parameters.Add(p => p.Cadence, cadence));
        Assert.Contains("Test Cadence", cut.Markup);
    }

    [TestMethod]
    public void CadenceTree_RendersCorrectly()
    {
        var cadence = new Cadence("Test Cadence", "Description", []);
        var cut = RenderComponent<CadenceTree>(parameters => parameters.Add(p => p.CadenceData, cadence));
        Assert.Contains("cadence-tree", cut.Markup);
    }

    [TestMethod]
    public void DropZone_RendersCorrectly()
    {
        var cut = RenderComponent<DropZone>();
        Assert.Contains("drop-zone", cut.Markup);
    }

    [TestMethod]
    public void DropZone_cadence_RendersCorrectly()
    {
        var cut = RenderComponent<DropZone_cadence>();
        Assert.Contains("drop-zone", cut.Markup);
    }

    [TestMethod]
    public void DropZone_item_RendersCorrectly()
    {
        var cut = RenderComponent<DropZone_item>();
        Assert.Contains("drop-zone", cut.Markup);
    }

    [TestMethod]
    public void Expander_RendersCorrectly()
    {
        var cut = RenderComponent<Expander>(parameters => parameters.AddChildContent("Test Content"));
        Assert.Contains("Test Content", cut.Markup);
    }

    [TestMethod]
    public void FeedbackPanel_RendersCorrectly()
    {
        var cut = RenderComponent<FeedbackPanel>();
        Assert.Contains("Submit Feedback", cut.Markup);
    }

    [TestMethod]
    public void HandPanel_RendersCorrectly()
    {
        var quest = new Quest("Test Quest", "Description");
        var locations = new List<LocationData> { new(new Location("Test Location", [quest]), [quest]) };
        var cut = RenderComponent<HandPanel>(parameters => parameters.Add(p => p.Locations, locations));
        Assert.Contains("Test Location", cut.Markup);
    }

    [TestMethod]
    public void InventoryItem_RendersCorrectly()
    {
        var item = new ItemQuantity(new Item("Test Item", "Description", ItemType.Material), 1);
        var cut = RenderComponent<InventoryItem>(parameters => parameters.Add(p => p.Item, item));
        Assert.Contains("Test Item", cut.Markup);
    }

    [TestMethod]
    public void PartyPanel_RendersCorrectly()
    {
        var party = new[] { new Character("Hero") };
        var cut = RenderComponent<PartyPanel>(parameters => parameters
            .Add(p => p.Party, party)
            .Add(p => p.QuestProgresses, [])
            .Add(p => p.Accepts, (obj) => true)
        );
        Assert.Contains("Party", cut.Markup);
    }

    [TestMethod]
    public void QuestCard_RendersCorrectly()
    {
        var quest = new Quest("Test Quest", "Description");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var questData = new QuestData(quest, detail);
        var cut = RenderComponent<QuestCard>(parameters => parameters.Add(p => p.QuestData, questData));
        Assert.Contains("Test Quest", cut.Markup);
        Assert.DoesNotContain("In Progress", cut.Markup);
        Assert.DoesNotContain("locked", cut.Markup);
    }

    [TestMethod]
    public void QuestCard_RendersInProgressCorrectly()
    {
        var quest = new Quest("Test Quest", "Description");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var questData = new QuestData(quest, detail);
        var character = new Character("Hero");

        // Mocking the progress in ResourceManager
        var resourceManager = Services.GetRequiredService<ResourceManager>();
        resourceManager.StartQuest(questData, character);

        var cut = RenderComponent<QuestCard>(parameters => parameters.Add(p => p.QuestData, questData));

        Assert.Contains("Test Quest", cut.Markup);
        Assert.Contains("In Progress", cut.Markup);
        Assert.Contains("locked", cut.Markup);
    }

    [TestMethod]
    public async Task QuestProgressCard_HandlesMultipleCompletions()
    {
        // Arrange
        var character = new Character("Hero");
        var quest = new Quest("Test Quest", "Description");
        var detail = new QuestDetail(1, [], [], QuestType.Recurring);
        var questData = new QuestData(quest, detail);
        var progress1 = new QuestProgress(questData, "Desc", 1, character);
        var progress2 = new QuestProgress(questData, "Desc", 1, character);

        var completionCount = 0;
        var cut = RenderComponent<QuestProgressCard>(parameters => parameters
            .Add(p => p.QuestProgress, progress1)
            .Add(p => p.OnCompletionAnimationEnd, () => completionCount++)
        );

        // Act - First completion
        progress1.SecondsElapsed = 1;
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.QuestProgress, progress1));
        await Task.Delay(1100); // Wait for the 1s delay in component

        // Assert
        Assert.AreEqual(1, completionCount);

        // Act - Second completion (simulating auto-quest restart)
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.QuestProgress, progress2));
        progress2.SecondsElapsed = 1;
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.QuestProgress, progress2));
        await Task.Delay(1100);

        // Assert
        Assert.AreEqual(2, completionCount);
    }

    [TestMethod]
    public void Snackbar_RendersCorrectly()
    {
        var cut = RenderComponent<Snackbar>();
        Assert.Contains("snackbar-container", cut.Markup);
    }

    [TestMethod]
    public void ThemeDiagnostics_RendersCorrectly()
    {
        var cut = RenderComponent<ThemeDiagnostics>();
        Assert.Contains("Theme Self-Diagnostic", cut.Markup);
    }

    [TestMethod]
    public void Workshop_RendersCorrectly()
    {
        var cut = RenderComponent<Workshop>();
        Assert.Contains("workshop-panel", cut.Markup);
    }

    [TestMethod]
    public void MainLayout_RendersCorrectly()
    {
        var cut = RenderComponent<MainLayout>();
        Assert.Contains("main", cut.Markup);
    }

    [TestMethod]
    public void Home_RendersCorrectly()
    {
        var cut = RenderComponent<Home>();
        Assert.Contains("theme-toggle", cut.Markup);
    }

    [TestMethod]
    public void TestRunner_RendersCorrectly()
    {
        var cut = RenderComponent<TestRunner>();
        Assert.Contains("AI-Driven Live Testing", cut.Markup);
    }

    [TestMethod]
    public void CharacterDisplay_AutoQuestToggle_RendersWhenUnlocked()
    {
        var character = ResourceManager.Characters[0];
        var recruit = Cadences.All.First(c => c.Name == "Recruit");

        ResourceManager.UnlockAbility("Recruit", "AutoQuest I");
        JunctionManager.AssignCadence(recruit, character, ResourceManager.UnlockedAbilities);

        // Act
        var cut = RenderComponent<CharacterDisplay>(parameters => parameters
            .Add(p => p.Character, character)
        );

        // Assert
        var toggle = cut.Find("[data-testid='autoquest-toggle']");
        Assert.IsNotNull(toggle);
        Assert.Contains("Auto: OFF", toggle.TextContent);

        // Toggle
        toggle.Click();
        Assert.Contains("Auto: ON", toggle.TextContent);
        Assert.IsTrue(ResourceManager.IsAutoQuestEnabled(character));
    }

    [TestMethod]
    public void App_RendersCorrectly()
    {
        var cut = RenderComponent<App>();
        Assert.IsTrue(cut.Markup.Contains("spinner-border") || cut.Markup.Contains("main"));
    }
}