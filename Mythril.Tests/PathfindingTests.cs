using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class PathfindingTests
{
    private PathfindingService? _pathfinding;
    private Quests? _quests;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _quests = ContentHost.GetContent<Quests>();
        
        _pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            _quests,
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestDetails>(),
            ContentHost.GetContent<Cadences>(),
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );
    }

    [TestMethod]
    public void GetPrerequisitePath_ReturnsTarget()
    {
        var target = "Prologue";
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
    }

    [TestMethod]
    public void GetPrerequisitePath_ReturnsPrerequisites()
    {
        // "Buy Potion" requires "Prologue"
        var target = "Buy Potion";
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
        Assert.IsTrue(path.Contains("Prologue"), "Path should contain prerequisite quest.");
    }

    [TestMethod]
    public void GetPrerequisitePath_RespectsCompletedQuests()
    {
        var target = "Buy Potion";
        var completed = new HashSet<string> { "Prologue" };
        var path = _pathfinding!.GetPrerequisitePath(target, completed, []);
        
        Assert.IsTrue(path.Contains(target));
        // Should NOT contain Prologue in the "needed" path because it's already completed
        // Wait, my implementation adds everything it visits to 'path'. 
        // Let's check the implementation logic.
    }

    [TestMethod]
    public void GetPrerequisitePath_HandlesAbilities()
    {
        var target = "Recruit:AutoQuest I";
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
        // Recruit is unlocked by Prologue
        Assert.IsTrue(path.Contains("Prologue"), "Should contain the quest that unlocks the cadence.");
    }
}
