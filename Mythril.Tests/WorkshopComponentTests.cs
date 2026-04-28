using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Mythril.Blazor.Components;
using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class WorkshopComponentTests : BunitTestBase
{
    [TestMethod]
    public void CharacterMiniIcon_RendersCorrectly()
    {
        // Arrange
        var character = new Character("Protagonist");

        // Act
        var cut = RenderComponent<CharacterMiniIcon>(parameters => parameters
            .Add(p => p.Character, character)
        );

        // Assert
        var icon = cut.Find("[data-testid='character-mini-icon-protagonist']");
        Assert.AreEqual("person", icon.TextContent.Trim());
        Assert.Contains("#ff4444", icon.GetAttribute("style"));
    }

    [TestMethod]
    public void RefinementCard_RendersCorrectly()
    {
        // Arrange
        // Use an ability that already exists in SandboxContent for Recruit
        var ability = ContentHost.GetContent<CadenceAbilities>().All.First(a => a.Name == SandboxContent.RefineScrap);
        var input = ContentHost.GetContent<Items>().All.First(i => i.Name == SandboxContent.Scrap);
        var output = ContentHost.GetContent<Items>().All.First(i => i.Name == SandboxContent.Gold);
        var recipe = new Recipe(5, output, 10);
        var refinement = new RefinementData(ability, input, recipe);

        var resourceManager = Services.GetRequiredService<ResourceManager>();
        var character = resourceManager.Characters[0]; // Protagonist

        // Ensure Recruit is unlocked so we can find it in UnlockedCadences
        var recruit = ContentHost.GetContent<Cadences>().All.First(c => c.Name == SandboxContent.Recruit);
        resourceManager.UnlockCadence(recruit);

        resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.RefineScrap);
        resourceManager.JunctionManager.AssignCadence(recruit, character, resourceManager.UnlockedAbilities);

        // Act - Render AFTER setup
        var cut = RenderComponent<RefinementCard>(parameters => parameters
            .Add(p => p.Refinement, refinement)
        );

        cut.Render();

        // Assert
        var text = cut.Find(".refinement-info").TextContent;
        Assert.Contains("5 Scrap", text);
        Assert.Contains("10 Gold", text);

        // Should see character mini icon
        var miniIcon = cut.Find("[data-testid^='character-mini-icon-']");
        Assert.IsNotNull(miniIcon);
        
        var iconHtml = cut.Find(".character-access").InnerHtml;
        Assert.Contains("person", iconHtml); // Protagonist icon
    }
}
