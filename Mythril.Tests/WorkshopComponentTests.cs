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

    [TestMethod]
    public void Workshop_Filtering_Works()
    {
        // Arrange
        var resourceManager = Services.GetRequiredService<ResourceManager>();
        var items = ContentHost.GetContent<Items>().All;
        var scrap = items.First(i => i.Name == SandboxContent.Scrap);
        var gold = items.First(i => i.Name == SandboxContent.Gold);
        var basicGem = items.First(i => i.Name == SandboxContent.BasicGem);
        var fireI = items.First(i => i.Name == SandboxContent.FireI);

        // Gold is Currency, Scrap is Material, BasicGem is Material, FireI is Spell
        Assert.AreEqual(ItemType.Currency, gold.ItemType);
        Assert.AreEqual(ItemType.Material, scrap.ItemType);
        Assert.AreEqual(ItemType.Material, basicGem.ItemType);
        Assert.AreEqual(ItemType.Spell, fireI.ItemType);

        // Setup abilities
        resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.RefineScrap);
        resourceManager.UnlockAbility(SandboxContent.Student, SandboxContent.RefineFire);

        var unlockedAbilities = new HashSet<string> { 
            $"{SandboxContent.Recruit}:{SandboxContent.RefineScrap}",
            $"{SandboxContent.Student}:{SandboxContent.RefineFire}"
        };

        // Act & Assert
        var cut = RenderComponent<Workshop>(parameters => parameters
            .Add(p => p.UnlockedAbilities, unlockedAbilities)
        );

        // 1. Default (All)
        var cards = cut.FindComponents<RefinementCard>();
        Assert.AreEqual(2, cards.Count);

        // 2. Filter by Magic
        var magicFilter = cut.Find("[data-testid='workshop-filter-magic']");
        magicFilter.Click();
        cards = cut.FindComponents<RefinementCard>();
        Assert.AreEqual(1, cards.Count);
        Assert.AreEqual(SandboxContent.RefineFire, cards[0].Instance.Refinement.Ability.Name);

        // 3. Filter by Materials
        var materialsFilter = cut.Find("[data-testid='workshop-filter-materials']");
        materialsFilter.Click();
        cards = cut.FindComponents<RefinementCard>();
        Assert.AreEqual(1, cards.Count);
        // RefineScrap outputs Gold (Currency) but inputs Scrap (Material), so it should show up under Materials
        Assert.AreEqual(SandboxContent.RefineScrap, cards[0].Instance.Refinement.Ability.Name);
    }
}
