using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using Mythril.Data.Materia;
using Mythril.GameLogic;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
{
    private ResourceManager resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var cards = new List<CardData>
        {
            new CardData { Id = "card1", Title = "Forest Foraging", DurationSeconds = 60, RewardValue = 10 }
        };
        var characters = new List<Character>
        {
            new Character("Hero", "Squire")
        };
        var materia = new List<Materia>
        {
            new MagicMateria("Fire", "Casts Fire spell", 100, 3, new List<string> { "Fire1", "Fire2", "Fire3" }),
            new SummonMateria("Shiva", "Summons Shiva", 500, 5, "Shiva")
        };
        resourceManager.SetData(cards, characters, materia, new List<Data.Jobs.Job>(), new List<Data.Items.Item>(), new List<Enemy>());
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(resourceManager.Cards);
        Assert.AreEqual(1, resourceManager.Cards.Count);
        Assert.IsNotNull(resourceManager.Characters);
        Assert.AreEqual(1, resourceManager.Characters.Count);
        Assert.IsNotNull(resourceManager.Materia);
        Assert.AreEqual(2, resourceManager.Materia.Count);
    }

    [TestMethod]
    public void ResourceManager_RetrievesCardData_Correctly()
    {
        // Assert
        var card = resourceManager.Cards.FirstOrDefault(c => c.Id == "card1");
        Assert.IsNotNull(card);
        Assert.AreEqual("Forest Foraging", card.Title);
        Assert.AreEqual(60, card.DurationSeconds);
        Assert.AreEqual(10, card.RewardValue);
    }

    [TestMethod]
    public void ResourceManager_RetrievesCharacterData_Correctly()
    {
        // Assert
        var character = resourceManager.Characters.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(character);
        Assert.AreEqual("Squire", character.JobName);
    }

    [TestMethod]
    public void ResourceManager_RetrievesMateriaData_Correctly()
    {
        // Assert
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
