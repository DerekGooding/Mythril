using Mythril.GameLogic;
using Mythril.GameLogic.Combat;

namespace Mythril.Tests;

[TestClass]
public class CombatTests
{
    [TestMethod]
    public void CombatManager_StartCombat_InitializesPartiesCorrectly()
    {
        // Arrange
        var resourceManager = new ResourceManager();
        var partyManager = new PartyManager(resourceManager);
        var combatManager = new CombatManager(partyManager);
        var enemies = new List<Character> { resourceManager.Enemies[0], resourceManager.Enemies[1] };

        // Act
        combatManager.StartCombat(enemies);

        // Assert
        Assert.AreEqual(4, combatManager.PlayerParty.Count);
        Assert.AreEqual(2, combatManager.EnemyParty.Count);
        Assert.AreEqual("Hero", combatManager.PlayerParty[0].Name);
        Assert.AreEqual("Goblin", combatManager.EnemyParty[0].Name);
    }
}
