using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Mythril.Blazor.Components;
using Mythril.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class RequirementIconTests : BunitTestBase
{
    [TestMethod]
    public void QuestCard_RendersRequirementIcons()
    {
        // Arrange
        var item = new Item("Stone", "Common stone", ItemType.Material);
        var quest = new Quest("Icon Test", "Desc");
        var detail = new QuestDetail(10, 
            [new ItemQuantity(item, 5)], 
            [], 
            QuestType.Single, 
            "Strength", 
            new Dictionary<string, int> { { "Strength", 20 } });
        
        var questData = new QuestData(quest, detail);

        // Act
        var cut = RenderComponent<QuestCard>(parameters => parameters
            .Add(p => p.QuestData, questData)
        );

        // Assert
        // Check for Item Icon
        var itemIcon = cut.Find("span[title='Item Requirement']");
        Assert.AreEqual("📦", itemIcon.TextContent.Trim());

        // Check for Stat Icon
        var statIcon = cut.Find("span[title='Stat Requirement']");
        Assert.AreEqual("🛡️", statIcon.TextContent.Trim());
    }

    [TestMethod]
    public void QuestCard_RendersUnlockIcon()
    {
        // Arrange
        var targetQuest = new Quest("Next Quest", "Desc");
        var quest = new Quest("Icon Test", "Desc");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var questData = new QuestData(quest, detail);

        // Inject prerequisite into the service: Next Quest requires Icon Test
        var questUnlocks = TestContext.Services.GetRequiredService<QuestUnlocks>();
        questUnlocks.Load(new Dictionary<Quest, Quest[]> { { targetQuest, [quest] } });

        // Act
        var cut = RenderComponent<QuestCard>(parameters => parameters
            .Add(p => p.QuestData, questData)
        );

        // Assert
        var unlockIcon = cut.Find("span[title='Unlocks']");
        Assert.AreEqual("✨", unlockIcon!.TextContent!.Trim());
        Assert.IsTrue(cut.Markup.Contains("unlock new quest"));
    }

    [TestMethod]
    public void AbilityUnlockCard_RendersItemIcon()
    {
        // Arrange
        var item = new Item("Spark", "Desc", ItemType.Material);
        var ability = new CadenceAbility("Super Jump", "Desc");
        var unlock = new CadenceUnlock("JumpMaster", ability, [new ItemQuantity(item, 1)]);

        // Act
        var cut = RenderComponent<AbilityUnlockCard>(parameters => parameters
            .Add(p => p.Unlock, unlock)
        );

        // Assert
        var itemIcon = cut.Find("span[title='Item Requirement']");
        Assert.AreEqual("📦", itemIcon!.TextContent!.Trim());
    }
}
