using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Mythril.Blazor.Components;
using Mythril.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class JournalTests : BunitTestBase
{
    [TestMethod]
    public void JournalPanel_RendersEmptyCorrectly()
    {
        // Act
        var cut = RenderComponent<JournalPanel>();

        // Assert
        var noEntries = cut.Find(".no-entries");
        Assert.IsTrue(noEntries.TextContent.Contains("No tasks completed yet."));
    }

    [TestMethod]
    public void JournalPanel_RendersEntriesCorrectly()
    {
        // Arrange
        ResourceManager.Journal.Add(new ResourceManager.JournalEntry("Test Task", "Hero", "Details here", DateTime.Now));

        // Act
        var cut = RenderComponent<JournalPanel>();

        // Assert
        var entry = cut.Find(".journal-entry");
        Assert.IsTrue(entry.TextContent.Contains("Test Task"));
        Assert.IsTrue(entry.TextContent.Contains("Hero"));
        Assert.IsTrue(entry.TextContent.Contains("Details here"));
    }
}
