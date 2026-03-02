using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Mythril.Blazor.Components;
using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class UIComponentTests : BunitTestContext
{
    [TestMethod]
    public void QuestProgressCard_RendersCorrectly()
    {
        // Arrange
        var mockJs = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJs.Object);

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
        var mockJs = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJs.Object);

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
}

public abstract class BunitTestContext : TestContextWrapper
{
    [TestInitialize]
    public void Setup() => TestContext = new Bunit.TestContext();

    [TestCleanup]
    public void TearDown() => TestContext?.Dispose();
}
