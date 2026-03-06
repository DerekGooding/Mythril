using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JunctionManagementTests
{
    private ResourceManager? _resourceManager;
    private JunctionManager? _junctionManager;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        var items = ContentHost.GetContent<Items>();
        var stats = ContentHost.GetContent<Stats>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        var inventory = new InventoryManager();
        _junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), _cadences);
        _resourceManager = new ResourceManager(
            items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            _junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>());
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void JunctionManager_AssignCadence_SupportsMultipleCadences()
    {
        var character = _resourceManager!.Characters[0];
        var cadence1 = _cadences!.All[0];
        var cadence2 = _cadences!.All[1];
        
        _junctionManager!.AssignCadence(cadence1, character, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadence2, character, _resourceManager.UnlockedAbilities);
        
        var assigned = _junctionManager.CurrentlyAssigned(character).ToList();
        Assert.AreEqual(2, assigned.Count);
        Assert.IsTrue(assigned.Contains(cadence1));
        Assert.IsTrue(assigned.Contains(cadence2));
    }

    [TestMethod]
    public void JunctionManager_AssignCadence_MaintainsExclusivity()
    {
        var character1 = _resourceManager!.Characters[0];
        var character2 = _resourceManager!.Characters[1];
        var cadence = _cadences!.All[0];
        
        _junctionManager!.AssignCadence(cadence, character1, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadence, character2, _resourceManager.UnlockedAbilities);
        
        Assert.IsFalse(_junctionManager.CurrentlyAssigned(character1).Contains(cadence));
        Assert.IsTrue(_junctionManager.CurrentlyAssigned(character2).Contains(cadence));
    }

    [TestMethod]
    public void JunctionManager_AssignCadence_Works()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First();
        
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        
        Assert.AreEqual(cadence, _junctionManager.CurrentlyAssigned(character).First());
    }

    [TestMethod]
    public void JunctionManager_CurrentlyAssigned_ReturnsCorrectCadences()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First();
        
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        var assigned = _junctionManager.CurrentlyAssigned(character);
        
        Assert.IsTrue(assigned.Contains(cadence));
    }

    [TestMethod]
    public void JunctionManager_Unassign_ClearsAssignments()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First();
        
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        _junctionManager.Unassign(cadence, _resourceManager.UnlockedAbilities);
        
        Assert.IsFalse(_junctionManager.CurrentlyAssigned(character).Any());
    }

    [TestMethod]
    public void JunctionManager_Unassign_UnassignedCadence()
    {
        var cadence = _cadences!.All.First();
        // Should not throw
        _junctionManager!.Unassign(cadence, _resourceManager!.UnlockedAbilities);
    }

    [TestMethod]
    public void JunctionManager_Unassign_InvalidatesJunctions()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        var strStat = ContentHost.GetContent<Stats>().All.First(s => s.Name == "Strength");
        var fire = ContentHost.GetContent<Items>().All.First(i => i.Name == "Fire I");

        _resourceManager.UnlockedAbilities.Add("Recruit:J-Str");
        _junctionManager!.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        
        _junctionManager.JunctionMagic(character, strStat, fire, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);

        _junctionManager.Unassign(recruit, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(0, _junctionManager.Junctions.Count, "Junction should be removed when ability is lost.");
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_DoesNothingWithoutAbility()
    {
        var character = _resourceManager!.Characters[0];
        var strStat = ContentHost.GetContent<Stats>().All.First(s => s.Name == "Strength");
        var fire = ContentHost.GetContent<Items>().All.First(i => i.Name == "Fire I");

        _junctionManager!.JunctionMagic(character, strStat, fire, _resourceManager!.UnlockedAbilities);
        Assert.AreEqual(0, _junctionManager.Junctions.Count);
    }
}
