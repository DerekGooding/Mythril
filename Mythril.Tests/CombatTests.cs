using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class CombatTests
{
    private ResourceManager? resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var characters = new List<Character>
        {
            new("Hero", "Squire"),
            new("Ally", "Chemist")
        };
        var enemies = new List<Enemy>
        {
            new("Goblin", "Warrior", "Test Zone"),
            new("Slime", "Monster", "Test Zone")
        };
        resourceManager.SetData([], characters, [], [], [], enemies);
    }

    [TestMethod]
    public void CombatManager_StartCombat_InitializesPartiesCorrectly()
    {
        // Arrange
        var partyManager = new PartyManager(resourceManager!);
        var combatManager = new CombatManager(partyManager, resourceManager!);
        var enemies = new List<Enemy> { resourceManager!.Enemies[0], resourceManager.Enemies[1] };

        // Act
        combatManager.StartCombat(enemies);

        // Assert
        Assert.HasCount(2, combatManager.PlayerParty);
        Assert.HasCount(2, combatManager.EnemyParty);
        Assert.AreEqual("Hero", combatManager.PlayerParty[0].Name);
        Assert.AreEqual("Goblin", combatManager.EnemyParty[0].Name);
    }

    [TestMethod]
    public void CombatManager_SimulateToEnd_PlayerWins()
    {
        // Arrange
        var partyManager = new PartyManager(resourceManager!);
        partyManager.PartyMembers.Clear();
        partyManager.AddPartyMember(resourceManager!.Characters[0]);
        var combatManager = new CombatManager(partyManager, resourceManager!);
        var enemies = new List<Enemy> { resourceManager!.Enemies[0] };
        resourceManager!.Characters[0].AttackPower = 100;
        resourceManager!.Enemies[0].Health = 10;
        resourceManager!.Enemies[0].AttackPower = 1;


        // Act
        combatManager.StartCombat(enemies);
        combatManager.SimulateToEnd();

        // Assert
        Assert.AreEqual(CombatState.Victory, combatManager.State);
        Assert.AreEqual(10, resourceManager!.Gold);
        Assert.AreEqual(10, resourceManager!.Characters[0].JobPoints);
    }
}
