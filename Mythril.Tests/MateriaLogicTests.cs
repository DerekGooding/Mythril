using Mythril.Data.Materia;

namespace Mythril.Tests;

[TestClass]
public class MateriaLogicTests
{
    private class TestMateria(string name, string description, int maxAP, int maxLevel) : Data.Materia.Materia(name, description, MateriaType.Magic, maxAP, maxLevel)
    {
    }

    [TestMethod]
    public void AddAP_LevelsUp_AndCarriesOverExcessAP()
    {
        // Arrange
        var materia = new TestMateria("Test", "A test materia", 100, 5); // Max AP = 100

        // Act
        materia.AddAP(250); // Add enough AP to level up twice with some left over

        // Assert
        Assert.AreEqual(3, materia.Level); // Should be level 3 (1 -> 2 -> 3)
        Assert.AreEqual(50, materia.AP);   // Should have 50 AP remaining (250 - 100 - 100)
    }

    [TestMethod]
    public void AddAP_DoesNotExceed_MaxLevel()
    {
        // Arrange
        var materia = new TestMateria("Test", "A test materia", 100, 2); // Max Level = 2

        // Act
        materia.AddAP(300);

        // Assert
        Assert.AreEqual(2, materia.Level); // Should be level 2
        Assert.AreEqual(100, materia.AP);  // Should not have lost the AP, but not leveled up further
    }
}
