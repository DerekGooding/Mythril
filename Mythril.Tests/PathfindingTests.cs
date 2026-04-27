using Mythril.Data;

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

        Assert.Contains(target, path);
    }

    [TestMethod]
    public void GetPrerequisitePath_ReturnsPrerequisites()
    {
        // Buy Potion -> Tutorial -> Prologue
        var target = SandboxContent.BuyPotion;
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);

        Assert.Contains(target, path);
        Assert.Contains(SandboxContent.Tutorial, path);
        Assert.Contains(SandboxContent.Prologue, path, "Path should contain prerequisite quest.");
    }

    [TestMethod]
    public void GetPrerequisitePath_RespectsCompletedQuests()
    {
        var target = SandboxContent.BuyPotion;
        var completed = new HashSet<string> { SandboxContent.Tutorial };
        var path = _pathfinding!.GetPrerequisitePath(target, completed, []);

        Assert.Contains(target, path);
        Assert.DoesNotContain(SandboxContent.Prologue, path, "Should not contain quests before already completed ones.");
    }

    [TestMethod]
    public void GetPrerequisitePath_HandlesAbilities()
    {
        var target = $"{SandboxContent.Recruit}:{SandboxContent.AutoQuestI}";
        var path = _pathfinding!.GetPrerequisitePath(target, [], []);

        Assert.Contains(target, path);
        // Recruit is unlocked by Prologue
        Assert.Contains(SandboxContent.Prologue, path, "Should contain the quest that unlocks the cadence.");
    }
}