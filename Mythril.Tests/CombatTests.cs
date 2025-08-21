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
            new("Goblin", "Warrior"),
            new("Slime", "Monster")
        };
        resourceManager.SetData([], characters, [], [], [], enemies);
    }

    [TestMethod]
    public void CombatManager_StartCombat_InitializesPartiesCorrectly()
    {
        // Arrange
        var partyManager = new PartyManager(resourceManager!);
        var combatManager = new CombatManager(partyManager);
        var enemies = new List<Character> { resourceManager!.Enemies[0], resourceManager.Enemies[1] };

        // Act
        combatManager.StartCombat(enemies);

        // Assert
        Assert.HasCount(2, combatManager.PlayerParty);
        Assert.HasCount(2, combatManager.EnemyParty);
        Assert.AreEqual("Hero", combatManager.PlayerParty[0].Name);
        Assert.AreEqual("Goblin", combatManager.EnemyParty[0].Name);
    }
}
