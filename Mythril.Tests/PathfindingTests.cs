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
        SandboxContent.Load();
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
        var target = SandboxContent.Prologue;
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
    }

    [TestMethod]
    public void GetPrerequisitePath_ReturnsPrerequisites()
    {
        // Buy Potion -> Tutorial -> Prologue
        var target = SandboxContent.BuyPotion;
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
        Assert.IsTrue(path.Contains(SandboxContent.Tutorial));
        Assert.IsTrue(path.Contains(SandboxContent.Prologue), "Path should contain prerequisite quest.");
    }

    [TestMethod]
    public void GetPrerequisitePath_RespectsCompletedQuests()
    {
        var target = SandboxContent.BuyPotion;
        var completed = new HashSet<string> { SandboxContent.Tutorial };
        var path = _pathfinding!.GetPrerequisitePath(target, completed, []);
        
        Assert.IsTrue(path.Contains(target));
        Assert.IsFalse(path.Contains(SandboxContent.Prologue), "Should not contain quests before already completed ones.");
    }

    [TestMethod]
    public void GetPrerequisitePath_HandlesAbilities()
    {
        var target = $"{SandboxContent.Recruit}:{SandboxContent.AutoQuestI}";
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);
        
        Assert.IsTrue(path.Contains(target));
        // Recruit is unlocked by Prologue
        Assert.IsTrue(path.Contains(SandboxContent.Prologue), "Should contain the quest that unlocks the cadence.");
    }
}
