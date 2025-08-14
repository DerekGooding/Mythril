using Mythril.GameLogic;
using Mythril.GameLogic.Materia;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
{
    [TestMethod]
    public void ResourceManager_LoadsData_OnConstruction()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        Assert.IsNotNull(resourceManager.Cards);
        Assert.IsTrue(resourceManager.Cards.Count > 0);
        Assert.IsNotNull(resourceManager.Characters);
        Assert.IsTrue(resourceManager.Characters.Count > 0);
    }

    [TestMethod]
    public void ResourceManager_LoadsCardData_Correctly()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        var card = resourceManager.Cards.FirstOrDefault(c => c.Id == "card1");
        Assert.IsNotNull(card);
        Assert.AreEqual("Forest Foraging", card.Title);
        Assert.AreEqual(60, card.DurationSeconds);
        Assert.AreEqual(10, card.RewardValue);
    }

    [TestMethod]
    public void ResourceManager_LoadsCharacterData_Correctly()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        var character = resourceManager.Characters.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(character);
        Assert.AreEqual("Squire", character.JobName);
    }

    [TestMethod]
    public void ResourceManager_LoadsMateriaData_Correctly()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        Assert.IsNotNull(resourceManager.Materia);
        Assert.IsTrue(resourceManager.Materia.Count > 0);

        var fireMateria = resourceManager.Materia.FirstOrDefault(m => m.Name == "Fire") as MagicMateria;
        Assert.IsNotNull(fireMateria);
        Assert.AreEqual(MateriaType.Magic, fireMateria.Type);
        Assert.AreEqual(3, fireMateria.Spells.Count);

        var shivaMateria = resourceManager.Materia.FirstOrDefault(m => m.Name == "Shiva") as SummonMateria;
        Assert.IsNotNull(shivaMateria);
        Assert.AreEqual(MateriaType.Summon, shivaMateria.Type);
        Assert.AreEqual("Shiva", shivaMateria.SummonName);
    }
}
