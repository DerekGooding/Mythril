using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
{
    private ResourceManager? _resourceManager;

    [TestInitialize]
    public void Setup()
    {
        _resourceManager = new ResourceManager();
        var locations = new List<Location>
        {
            new()
            {
                Name = "Test Location",
                Quests =
                [
                    new() { Id = "card1", Title = "Forest Foraging", DurationSeconds = 60, RewardValue = 10 }
                ]
            }
        };
        var characters = new List<Character>
        {
            new("Hero")
        };
        _resourceManager.SetData(locations, characters, [], []);
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.Locations);
        Assert.AreEqual(1, _resourceManager.Locations.Count);
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.AreEqual(1, _resourceManager.Characters.Count);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var location = _resourceManager!.Locations[0];
        var task = location.Quests.FirstOrDefault(c => c.Id == "quest1");
        Assert.IsNotNull(task);
        Assert.AreEqual("Forest Foraging", task.Title);
        Assert.AreEqual(60, task.DurationSeconds);
        Assert.AreEqual(10, task.RewardValue);
    }

}
