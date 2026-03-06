using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using Mythril.Headless.Simulation;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class SimulationTests
{
    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
    }

    [TestMethod]
    public void RoutedSimulator_Step_Works()
    {
        var items = ContentHost.GetContent<Items>();
        var quests = ContentHost.GetContent<Quests>();
        var questDetails = ContentHost.GetContent<QuestDetails>();
        var questUnlocks = ContentHost.GetContent<QuestUnlocks>();
        var questToCadenceUnlocks = ContentHost.GetContent<QuestToCadenceUnlocks>();
        var cadences = ContentHost.GetContent<Cadences>();
        var locations = ContentHost.GetContent<Locations>();
        var refinements = ContentHost.GetContent<ItemRefinements>();
        var statAugments = ContentHost.GetContent<StatAugments>();
        var stats = ContentHost.GetContent<Stats>();

        var simulator = new RoutedSimulator(items, quests, questDetails, questUnlocks, questToCadenceUnlocks, cadences, locations, refinements, statAugments, stats);
        
        // This is a complex method, we just want to ensure it runs without crashing and covers lines
        simulator.Run();
    }
}
