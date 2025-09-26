using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManagerTests
{
    private ResourceManager? _resourceManager;

    [TestInitialize]
    public void Setup() => _resourceManager = new ResourceManager();

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.UsableLocations);
        Assert.HasCount(1, _resourceManager.UsableLocations);
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.AreEqual(1, _resourceManager.Characters.Length);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var location = _resourceManager!.UsableLocations[0];
        var task = location.Quests.FirstOrDefault(c => c.Name == "quest1");
        Assert.AreEqual("Forest Foraging", task.Name);
    }

}
