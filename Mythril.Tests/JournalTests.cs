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
        ResourceManager.Journal.Add(new ResourceManager.JournalEntry("Test Task", "Hero", "Details here", DateTime.Now, true));

        // Act
        var cut = RenderComponent<JournalPanel>();

        // Assert
        var entry = cut.Find(".journal-entry");
        Assert.IsTrue(entry.TextContent.Contains("Test Task"));
        Assert.IsTrue(entry.TextContent.Contains("Hero"));
        Assert.IsTrue(entry.TextContent.Contains("Details here"));
        Assert.IsTrue(entry.TextContent.Contains("First Time"));
    }

    [TestMethod]
    public void JournalPanel_Filter_WorksCorrectly()
    {
        // Arrange
        ResourceManager.Journal.Add(new ResourceManager.JournalEntry("First Task", "Hero", "First", DateTime.Now, true));
        ResourceManager.Journal.Add(new ResourceManager.JournalEntry("Second Task", "Hero", "Second", DateTime.Now, false));

        // Act
        var cut = RenderComponent<JournalPanel>();
        
        // Initially should show both
        Assert.AreEqual(2, cut.FindAll(".journal-entry").Count);

        // Toggle filter
        var checkbox = cut.Find("#firstTimeFilter");
        checkbox.Change(true);

        // Assert
        Assert.AreEqual(1, cut.FindAll(".journal-entry").Count);
        Assert.IsTrue(cut.Find(".journal-entry").TextContent.Contains("First Task"));
    }
}
