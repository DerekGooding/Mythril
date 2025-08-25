using Mythril.Data;
using Mythril.Data.Jobs;

namespace Mythril.Tests;

[TestClass]
public class JobTests
{
    private ResourceManager? _resourceManager;

    [TestInitialize]
    public void Setup()
    {
        _resourceManager = new ResourceManager();
        var jobs = new List<Job>
        {
            new Squire("Squire", "A basic warrior in training.", ["Tackle", "Throw Stone", "Heal"], 10, 2, 1, [100, 200])
        };
        var characters = new List<Character>
        {
            new("Hero")
        };
        _resourceManager.SetData([], characters, jobs, []);
    }

    [TestMethod]
    public void PartyManager_LinksJobsToCharacters_Correctly()
    {
        // Act
        var partyManager = new PartyManager(_resourceManager!);

        // Assert
        var hero = partyManager.PartyMembers.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(hero);
        Assert.IsNotNull(hero.Job);
        Assert.AreEqual("Squire", hero.Job.Name);
        Assert.IsInstanceOfType<Squire>(hero.Job);
    }
}
