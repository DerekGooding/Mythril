using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class JunctionStatTests
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
        SandboxContent.Load();
        _items = ContentHost.GetContent<Items>();
        _stats = ContentHost.GetContent<Stats>();
        _cadences = ContentHost.GetContent<Cadences>();
        _abilities = ContentHost.GetContent<CadenceAbilities>();
        
        var gameStore = new GameStore();
        var inventory = new InventoryManager(gameStore);
        _junctionManager = new JunctionManager(gameStore, inventory, ContentHost.GetContent<StatAugments>(), _cadences);

        var pathfinding = new PathfindingService(
            ContentHost.GetContent<Locations>(),
            ContentHost.GetContent<Quests>(),
            ContentHost.GetContent<QuestUnlocks>(),
            ContentHost.GetContent<QuestDetails>(),
            _cadences,
            ContentHost.GetContent<QuestToCadenceUnlocks>()
        );

        var quests = ContentHost.GetContent<Quests>();

        _resourceManager = new ResourceManager(gameStore, _items, quests, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            ContentHost.GetContent<QuestDetails>(), 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            _junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>(),
            pathfinding);
        _resourceManager.Initialize();
        _resourceManager.Inventory.Clear();
    }

    [TestMethod]
    public void JunctionManager_Unassign_OnlyClearsInvalidJunctions()
    {
        var character = _resourceManager!.Characters[0];
        var abilityJStr = _abilities!.All.First(a => a.Name == SandboxContent.JStr);
        var cadence1 = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var cadence2 = new Cadence("Extra Cadence", "Desc", [new CadenceUnlock("Extra Cadence", abilityJStr, [])]);
        _cadences.Load(_cadences.All.Concat([cadence2]));
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager.UnlockAbility("Extra Cadence", SandboxContent.JStr);
        _resourceManager.UnlockAbility(cadence1.Name, SandboxContent.JStr);
        _junctionManager!.AssignCadence(cadence1, character, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadence2, character, _resourceManager.UnlockedAbilities);
        
        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);

        // Unassign one cadence with the ability, but the other still has it
        _junctionManager.Unassign(cadence1, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);

        // Unassign the last cadence with the ability, junction should be gone
        _junctionManager.Unassign(cadence2, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(0, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var abilityJStr = _abilities!.All.First(a => a.Name == SandboxContent.JStr);
        var cadenceWithoutJStr = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var cadenceWithJStr = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager.UnlockAbility(cadenceWithJStr.Name, SandboxContent.JStr);
        _junctionManager!.AssignCadence(cadenceWithoutJStr, character, _resourceManager.UnlockedAbilities);
        _junctionManager!.AssignCadence(cadenceWithJStr, character, _resourceManager.UnlockedAbilities);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        Assert.AreEqual(1, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void ResourceManager_CanAutoQuest_ChecksAllAssignedCadences()
    {
        var character = _resourceManager!.Characters[0];
        var autoQuestAbility = _abilities!.All.First(a => a.Name == SandboxContent.AutoQuestI);
        var cadenceWithoutAuto = _cadences!.All.First(c => c.Name == SandboxContent.Arcanist);
        var cadenceWithAuto = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.AutoQuestI));

        _resourceManager.UnlockAbility(cadenceWithAuto.Name, SandboxContent.AutoQuestI);
        
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        _junctionManager!.AssignCadence(cadenceWithoutAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));

        _junctionManager!.AssignCadence(cadenceWithAuto, character, _resourceManager.UnlockedAbilities);
        Assert.IsTrue(_resourceManager.CanAutoQuest(character));
    }

    [TestMethod]
    public void ResourceManager_UnlockedAbilities_ArePerCadence()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);

        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);

        _junctionManager!.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        Assert.IsTrue(_resourceManager.CanAutoQuest(character));

        _junctionManager.Unassign(recruit, _resourceManager.UnlockedAbilities);
        _junctionManager.AssignCadence(apprentice, character, _resourceManager.UnlockedAbilities);
        
        Assert.IsFalse(_resourceManager.CanAutoQuest(character));
    }

    [TestMethod]
    public void JunctionManager_GetStatValue_BaseValues()
    {
        var protagonist = _resourceManager!.Characters.First(c => c.Name == "Protagonist");
        var strength = _junctionManager!.GetStatValue(protagonist, SandboxContent.Strength);
        Assert.AreEqual(10, strength);

        var wifu = _resourceManager.Characters.First(c => c.Name == "Wifu");
        var magic = _junctionManager.GetStatValue(wifu, SandboxContent.Magic);
        Assert.AreEqual(10, magic);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_IncreasesStats()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager.UnlockAbility(cadence.Name, SandboxContent.JStr);
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        
        // Add 40 units (within 30 capacity for spells, wait, Fire I is a spell)
        // If Fire I is a spell, capacity is 30.
        // Let's use 20 units to be safe.
        _resourceManager.Inventory.Add(fireMagic, 20);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, SandboxContent.Strength);
        // Base 10 + 1 (J-Str boost) + (20 items / 10) = 10 + 1 + 2 = 13
        Assert.AreEqual(13, val); 
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_FallbackLogic()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var log = _items!.All.First(i => i.Name == SandboxContent.Log);

        _resourceManager.UnlockAbility(cadence.Name, SandboxContent.JStr);
        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        
        // Add 10 units
        _resourceManager.Inventory.Add(log, 10);

        _junctionManager.JunctionMagic(character, strengthStat, log, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, SandboxContent.Strength);
        // Base 10 + 1 (J-Str boost) + (10 / 10) = 12
        Assert.AreEqual(12, val);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_RequiresAbility()
    {
        var character = _resourceManager!.Characters[0];
        var cadence = _cadences!.All.First(c => c.Abilities.Any(a => a.Ability.Name == SandboxContent.JStr));
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _junctionManager!.AssignCadence(cadence, character, _resourceManager.UnlockedAbilities);
        _resourceManager.Inventory.Add(fireMagic, 10);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, SandboxContent.Strength);
        Assert.AreEqual(10, val);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_NoCadence()
    {
        var character = _resourceManager!.Characters[0];
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _junctionManager!.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);
        
        Assert.AreEqual(0, _junctionManager.Junctions.Count);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_StrictlyRequiresCorrectAbility()
    {
        var character = _resourceManager!.Characters[0];
        // Apprentice has "AutoQuest I" but NOT "J-Str"
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager.UnlockAbility(SandboxContent.Apprentice, SandboxContent.AutoQuestI);
        _junctionManager!.AssignCadence(apprentice, character, _resourceManager.UnlockedAbilities);

        // Act: Try to junction Strength magic.
        // It should FAIL because Apprentice has abilities, but not J-Str.
        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        // Assert
        Assert.AreEqual(0, _junctionManager.Junctions.Count, "Should not allow junctioning with the wrong ability.");
    }

    [TestMethod]
    public void ResourceManager_UpdateMagicCapacity_Works()
    {
        Assert.AreEqual(30, _resourceManager!.Inventory.MagicCapacity);
        
        _resourceManager.UnlockAbility(SandboxContent.Arcanist, SandboxContent.MagicPocketI);
        Assert.AreEqual(60, _resourceManager.Inventory.MagicCapacity);

        _resourceManager.UnlockAbility(SandboxContent.Sentinel, SandboxContent.MagicPocketII);
        Assert.AreEqual(100, _resourceManager.Inventory.MagicCapacity);
    }
}
