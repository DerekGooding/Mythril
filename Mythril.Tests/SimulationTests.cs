using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using Mythril.Headless.Simulation;
using System.Collections.Generic;

namespace Mythril.Tests;

[TestClass]
public class SimulationTests
{
    [TestMethod]
    public void SimulationState_Initialization_Works()
    {
        // Arrange
        var stats = new Stats();
        stats.Load([
            new Stat("Strength", "STR"),
            new Stat("Magic", "MAG"),
            new Stat("Vitality", "VIT"),
            new Stat("Speed", "SPD")
        ]);

        // Act
        var state = new SimulationState(stats);

        // Assert
        Assert.AreEqual(0, state.CurrentTime);
        Assert.AreEqual(30, state.MagicCapacity);
        Assert.IsTrue(state.UnlockedCadences.Contains("Recruit"));
        Assert.AreEqual(25, state.CurrentStats["Strength"]);
        Assert.AreEqual(25, state.CurrentStats["Magic"]);
        Assert.AreEqual(25, state.CurrentStats["Vitality"]);
        Assert.AreEqual(25, state.CurrentStats["Speed"]);
    }

    [TestMethod]
    public void ActivitySource_Properties_Work()
    {
        // Arrange
        var quest = new Quest("Test Quest", "Desc");
        var detail = new QuestDetail(10, [], [], QuestType.Single);
        var ability = new CadenceAbility("Test Ability", "Desc");
        var recipes = new Dictionary<Item, Recipe>();

        // Act
        var source = new ActivitySource
        {
            Quest = quest,
            Detail = detail,
            Ability = ability,
            PrimaryStat = "Strength",
            Recipes = recipes
        };

        // Assert
        Assert.AreEqual(quest, source.Quest);
        Assert.AreEqual(detail, source.Detail);
        Assert.AreEqual(ability, source.Ability);
        Assert.AreEqual("Strength", source.PrimaryStat);
        Assert.AreEqual(recipes, source.Recipes);
    }
}
