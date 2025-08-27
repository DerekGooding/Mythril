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
                Tasks = new List<TaskData>
                {
                    new() { Id = "card1", Title = "Forest Foraging", DurationSeconds = 60, RewardValue = 10 }
                }
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
        var location = _resourceManager!.Locations.First();
        var task = location.Tasks.FirstOrDefault(c => c.Id == "card1");
        Assert.IsNotNull(task);
        Assert.AreEqual("Forest Foraging", task.Title);
        Assert.AreEqual(60, task.DurationSeconds);
        Assert.AreEqual(10, task.RewardValue);
    }

    [TestMethod]
    public void ResourceManager_RetrievesCharacterData_Correctly()
    {
        // Arrange
        var partyManager = new PartyManager(_resourceManager!);

        // Assert
        var character = partyManager.PartyMembers.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(character);
        Assert.AreEqual("Hero", character.JobName);
    }

    [TestMethod]
    public void UpgradeCharacterAttack_SufficientGold_UpgradesAttackAndDeductsGold()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        _resourceManager.AddGold(100);

        // Act
        var result = _resourceManager.UpgradeCharacterAttack(character);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(11, character.AttackPower);
        Assert.AreEqual(0, _resourceManager.Gold);
    }

    [TestMethod]
    public void UpgradeCharacterAttack_InsufficientGold_DoesNotUpgrade()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        _resourceManager.AddGold(50);

        // Act
        var result = _resourceManager.UpgradeCharacterAttack(character);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(10, character.AttackPower);
        Assert.AreEqual(50, _resourceManager.Gold);
    }
}
