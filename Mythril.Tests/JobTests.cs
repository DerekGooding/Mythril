using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data.Jobs;
using Mythril.GameLogic;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class JobTests
{
    private ResourceManager resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var jobs = new List<Job>
        {
            new Squire("Squire", "A basic warrior in training.", new List<string> { "Tackle", "Throw Stone", "Heal" })
        };
        var characters = new List<Mythril.Data.Character>
        {
            new Mythril.Data.Character("Hero", "Squire")
        };
        resourceManager.SetData(new List<Mythril.Data.CardData>(), characters, new List<Mythril.Data.Materia.Materia>(), jobs, new List<Mythril.Data.Items.Item>(), new List<Mythril.Data.Enemy>());
    }

    [TestMethod]
    public void PartyManager_LinksJobsToCharacters_Correctly()
    {
        // Act
        var partyManager = new PartyManager(resourceManager);

        // Assert
        var hero = partyManager.PartyMembers.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(hero);
        Assert.IsNotNull(hero.Job);
        Assert.AreEqual("Squire", hero.Job.Name);
        Assert.IsInstanceOfType(hero.Job, typeof(Squire));
    }
}
