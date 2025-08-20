using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using Mythril.GameLogic;
using Mythril.GameLogic.Combat;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class CombatTests
{
    private ResourceManager resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var characters = new List<Character>
        {
            new Character("Hero", "Squire"),
            new Character("Ally", "Chemist")
        };
        var enemies = new List<Enemy>
        {
            new Enemy("Goblin", "Warrior"),
            new Enemy("Slime", "Monster")
        };
        resourceManager.SetData(new List<CardData>(), characters, new List<Data.Materia.Materia>(), new List<Data.Jobs.Job>(), new List<Data.Items.Item>(), enemies);
    }

    [TestMethod]
    public void CombatManager_StartCombat_InitializesPartiesCorrectly()
    {
        // Arrange
        var partyManager = new PartyManager(resourceManager);
        var combatManager = new CombatManager(partyManager);
        var enemies = new List<Character> { resourceManager.Enemies[0], resourceManager.Enemies[1] };

        // Act
        combatManager.StartCombat(enemies);

        // Assert
        Assert.AreEqual(2, combatManager.PlayerParty.Count);
        Assert.AreEqual(2, combatManager.EnemyParty.Count);
        Assert.AreEqual("Hero", combatManager.PlayerParty[0].Name);
        Assert.AreEqual("Goblin", combatManager.EnemyParty[0].Name);
    }
}
