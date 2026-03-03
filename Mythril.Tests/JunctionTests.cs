using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JunctionTests
{
    private ResourceManager? _resourceManager;
    private JunctionManager? _junctionManager;
    private Items? _items;
    private Stats? _stats;
    private Cadences? _cadences;
    private CadenceAbilities? _abilities;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _stats = ContentHost.GetContent<Stats>();
        _cadences = ContentHost.GetContent<Cadences>();
        _abilities = ContentHost.GetContent<CadenceAbilities>();
        
        var inventory = new InventoryManager();
        _junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), _cadences);
        _resourceManager = new ResourceManager(
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            _junctionManager,
            inventory);
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
    public void JunctionManager_Unassign_OnlyClearsInvalidJunctions()
    {
        var character = _resourceManager!.Characters[0];
        var abilityJStr = _abilities!.All.First(a => a.Name == "J-Str");
        var cadence1 = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "J-Str"));
        var cadence2 = new Cadence("Extra Cadence", "Desc", [new CadenceUnlock(abilityJStr, [])]);
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");

        _resourceManager.UnlockedAbilities.Add(abilityJStr);
        _junctionManager!.AssignCadence(cadence1, character, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadence2, character, _resourceManager.UnlockedAbilities);
        
        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);

        // Unassign one cadence with the ability, but the other still has it
        _junctionManager.Unassign(cadence1);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);

        // Unassign the last cadence with the ability, junction should be gone
        _junctionManager.Unassign(cadence2);
        Assert.AreEqual(0, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var abilityJStr = _abilities!.All.First(a => a.Name == "J-Str");
        // Arcanist has Refine Ice, J-Magic, Magic Pocket I. No J-Str.
        var cadenceWithoutJStr = _cadences!.All.First(c => c.Name == "The Arcanist");
        var cadenceWithJStr = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "J-Str"));
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");

        _resourceManager.UnlockedAbilities.Add(abilityJStr);
        _junctionManager!.AssignCadence(cadenceWithoutJStr, character, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadenceWithJStr, character, _resourceManager.UnlockedAbilities);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void ResourceManager_CanAutoQuest_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var autoQuestAbility = _abilities!.All.First(a => a.Name == "AutoQuest I");
        // Arcanist does NOT have AutoQuest I
        var cadenceWithoutAuto = _cadences!.All.First(c => c.Name == "The Arcanist");
        var cadenceWithAuto = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "AutoQuest I"));

        _resourceManager.UnlockedAbilities.Add(autoQuestAbility);
        
        // No cadence
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        // Cadence without auto-quest
        _junctionManager!.AssignCadence(cadenceWithoutAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        // Add cadence with auto-quest
        _junctionManager!.AssignCadence(cadenceWithAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsTrue(_resourceManager.CanAutoQuest(character));
    }

    [TestMethod]
    public void JunctionManager_GetStatValue_BaseValues()
    {
        var protagonist = _resourceManager!.Characters.First(c => c.Name == "Protagonist");
        var strength = _junctionManager!.GetStatValue(protagonist, "Strength");
        Assert.AreEqual(10, strength);

        var wifu = _resourceManager.Characters.First(c => c.Name == "Wifu");
        var magic = _junctionManager.GetStatValue(wifu, "Magic");
        Assert.AreEqual(10, magic);
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
    public void JunctionManager_JunctionMagic_IncreasesStats()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "J-Str"));
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");

        // Unlock J-Str
        _resourceManager.UnlockedAbilities.Add(_abilities!.All.First(a => a.Name == "J-Str"));
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);

        // Add Fire I to inventory
        _resourceManager.Inventory.Add(fireMagic, 100);

        // Junction
        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, "Strength");
        // Base 10 + 30 (capped) * (10 / 100.0) = 10 + 3 = 13
        Assert.AreEqual(13, val);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_FallbackLogic()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "J-Str"));
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var log = _items!.All.First(i => i.Name == "Log"); // Not in stat_augments.json

        // Hack log to be a spell for testing junction
        var logSpell = log with { ItemType = ItemType.Spell };

        _resourceManager.UnlockedAbilities.Add(_abilities!.All.First(a => a.Name == "J-Str"));
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        _resourceManager.Inventory.Add(logSpell, 20);

        _junctionManager.JunctionMagic(character, strengthStat, logSpell, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, "Strength");
        // Base 10 + 20 / 10 = 12
        Assert.AreEqual(12, val);
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
        _junctionManager.Unassign(cadence);
        
        Assert.IsFalse(_junctionManager.CurrentlyAssigned(character).Any());
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_RequiresAbility()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == "J-Str"));
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");

        // Do NOT unlock J-Str
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        _resourceManager.Inventory.Add(fireMagic, 10);

        // Junction should fail silently
        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, "Strength");
        Assert.AreEqual(10, val); // Base value
    }

    [TestMethod]
    public void JunctionManager_Unassign_UnassignedCadence()
    {
        var cadence = _cadences!.All.First();
        // Should not throw
        _junctionManager!.Unassign(cadence);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_NoCadence()
    {
        var character = _resourceManager!.Characters[0];
        var strengthStat = _stats!.All.First(s => s.Name == "Strength");
        var fireMagic = _items!.All.First(i => i.Name == "Fire I");

        // Character has no cadence
        _junctionManager!.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        
        Assert.AreEqual(0, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void ResourceManager_UpdateMagicCapacity_Works()
    {
        Assert.AreEqual(30, _resourceManager!.Inventory.MagicCapacity);
        
        _resourceManager.UnlockedAbilities.Add(_abilities!.All.First(a => a.Name == "Magic Pocket I"));
        _resourceManager.UpdateMagicCapacity();
        
        Assert.AreEqual(60, _resourceManager.Inventory.MagicCapacity);
    }
}
