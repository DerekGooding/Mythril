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
                    new("Forest Foraging") { DurationSeconds = 60 }
                ]
            }
        };
        var characters = new Character[]
        {
            new("Hero")
        };
        _resourceManager.SetData(characters);
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.Locations);
        Assert.AreEqual(1, _resourceManager.Locations.Length);
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.AreEqual(1, _resourceManager.Characters.Length);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var location = _resourceManager!.Locations[0];
        var task = location.Quests.FirstOrDefault(c => c.Title == "quest1");
        Assert.IsNotNull(task);
        Assert.AreEqual("Forest Foraging", task.Title);
        Assert.AreEqual(60, task.DurationSeconds);
    }

}
