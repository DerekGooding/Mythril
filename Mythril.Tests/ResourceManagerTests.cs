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
                    new("Forest Foraging", "", 60)
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
        Assert.IsNotNull(_resourceManager!.UsableLocations);
        Assert.AreEqual(1, _resourceManager.UsableLocations.Count());
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.AreEqual(1, _resourceManager.Characters.Length);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var location = _resourceManager!.UsableLocations.First();
        var task = location.Quests.FirstOrDefault(c => c.Name == "quest1");
        Assert.IsNotNull(task);
        Assert.AreEqual("Forest Foraging", task.Name);
        Assert.AreEqual(60, task.DurationSeconds);
    }

}
