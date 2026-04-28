using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JunctionStatTests
{
    private ResourceManager? _resourceManager;
    private JunctionManager? _junctionManager;
    private Items? _items;
    private Stats? _stats;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        SandboxContent.Load();
        _items = ContentHost.GetContent<Items>();
        _stats = ContentHost.GetContent<Stats>();
        _cadences = ContentHost.GetContent<Cadences>();

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

        _resourceManager = new ResourceManager(gameStore, _items, ContentHost.GetContent<Quests>(),
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
        _resourceManager.Inventory.Add(fireMagic, 20);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, SandboxContent.Strength);
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
        _resourceManager.Inventory.Add(log, 10);

        _junctionManager.JunctionMagic(character, strengthStat, log, _resourceManager.UnlockedAbilities);

        var val = _junctionManager.GetStatValue(character, SandboxContent.Strength);
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

        Assert.IsEmpty(_junctionManager.Junctions);
    }

    [TestMethod]
    public void JunctionManager_JunctionMagic_StrictlyRequiresCorrectAbility()
    {
        var character = _resourceManager!.Characters[0];
        var apprentice = _cadences!.All.First(c => c.Name == SandboxContent.Apprentice);
        var strengthStat = _stats!.All.First(s => s.Name == SandboxContent.Strength);
        var fireMagic = _items!.All.First(i => i.Name == SandboxContent.FireI);

        _resourceManager.UnlockAbility(SandboxContent.Apprentice, SandboxContent.AutoQuestI);
        _junctionManager!.AssignCadence(apprentice, character, _resourceManager.UnlockedAbilities);

        _junctionManager.JunctionMagic(character, strengthStat, fireMagic, _resourceManager.UnlockedAbilities);

        Assert.IsEmpty(_junctionManager.Junctions, "Should not allow junctioning with the wrong ability.");
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
