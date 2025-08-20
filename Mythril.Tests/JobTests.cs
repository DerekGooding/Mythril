using Mythril.GameLogic;
using Mythril.GameLogic.Jobs;

namespace Mythril.Tests;

[TestClass]
public class JobTests
{
    [TestMethod]
    public void ResourceManager_LoadsJobData_Correctly()
    {
        // Act
        var resourceManager = new ResourceManager();

        // Assert
        Assert.IsNotNull(resourceManager.Jobs);
        Assert.IsGreaterThan(0, resourceManager.Jobs.Count);

        var squireJob = resourceManager.Jobs.FirstOrDefault(j => j.Name == "Squire") as Squire;
        Assert.IsNotNull(squireJob);
        Assert.AreEqual("A basic warrior in training.", squireJob.Description);
        Assert.HasCount(3, squireJob.Abilities);
    }

    [TestMethod]
    public void PartyManager_LinksJobsToCharacters_Correctly()
    {
        // Arrange
        var resourceManager = new ResourceManager();

        // Act
        var partyManager = new PartyManager(resourceManager);

        // Assert
        var hero = partyManager.PartyMembers.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(hero);
        Assert.IsNotNull(hero.Job);
        Assert.AreEqual("Squire", hero.Job.Name);
        Assert.IsInstanceOfType<Squire>(hero.Job);
    }
}
