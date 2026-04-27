using Mythril.Data;
using Mythril.Headless.Simulation;
using System.Collections.Immutable;

namespace Mythril.Tests;

[TestClass]
public class LatticeSimulatorTests : BunitTestBase
{
    private LatticeSimulator? _lattice;

    [TestInitialize]
    public void SetupLattice()
    {
        SandboxContent.Load();
        _lattice = new LatticeSimulator(
            ContentHost.GetContent<Items>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestDetails>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestToCadenceUnlocks>(),
            ContentHost.GetContent<Cadences>(),
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<ItemRefinements>(),
            ContentHost.GetContent<StatAugments>(),
            ContentHost.GetContent<Stats>()
        );
    }

    [TestMethod]
    public void Join_IsIdempotent()
    {
        var state = _lattice!.Bottom(new SimulationSeed([], Stats.All.ToImmutableDictionary(s => s.Name, _ => 10), [], []));

        var join1 = _lattice.Join(state, state);

        Assert.AreEqual(state, join1);
    }

    [TestMethod]
    public void Join_IsCommutative()
    {
        var a = _lattice!.Bottom(new SimulationSeed([], Stats.All.ToImmutableDictionary(s => s.Name, _ => 10), [], []));
        var b = a with { MagicCapacity = 100 };

        var join1 = _lattice.Join(a, b);
        var join2 = _lattice.Join(b, a);

        Assert.AreEqual(join1, join2);
        Assert.AreEqual(100, join1.MagicCapacity);
    }

    [TestMethod]
    public void Solver_ConvergesOnCircularQuestDependency()
    {
        var seed = new SimulationSeed(
            [],
            Stats.All.ToImmutableDictionary(s => s.Name, _ => 10),
            [SandboxContent.Recruit],
            []
        );

        var result = _lattice!.Solve(seed);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void Solver_PropagatesResourceToRefinement()
    {
        // 1. Setup seed where we have Scrap and Refine Scrap unlocked
        var seed = new SimulationSeed(
            new Dictionary<string, int> { { SandboxContent.Scrap, 10 } }.ToImmutableDictionary(),
            Stats.All.ToImmutableDictionary(s => s.Name, _ => 10),
            [SandboxContent.Recruit],
            [$"{SandboxContent.Recruit}:{SandboxContent.RefineScrap}"]
        );

        // 2. Solve
        var result = _lattice!.Solve(seed);

        // 3. Assert - Gold should be reachable
        Assert.AreNotEqual(double.PositiveInfinity, result.ResourceTime[SandboxContent.Gold], "Gold should be reachable via refinement when Scrap is available.");
    }

    [TestMethod]
    public void StatGate_PreventsUnreachableContent()
    {
        // Sandbox Scholar needs 100 Magic
        var seed = new SimulationSeed(
            [],
            Stats.All.ToImmutableDictionary(s => s.Name, _ => 10),
            [SandboxContent.Recruit],
            []
        );

        // Act
        var result = _lattice!.Solve(seed);

        // Assert
        Assert.DoesNotContain(SandboxContent.Scholar, result.UnlockedCadences);
    }
}