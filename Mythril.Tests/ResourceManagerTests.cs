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
        var tasks = new List<TaskData>
        {
            new() { Id = "card1", Title = "Forest Foraging", DurationSeconds = 60, RewardValue = 10 }
        };
        var characters = new List<Character>
        {
            new("Hero")
        };
        _resourceManager.SetData(tasks, characters, [], []);
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.Tasks);
        Assert.HasCount(1, _resourceManager.Tasks);
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.HasCount(1, _resourceManager.Characters);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var task = _resourceManager!.Tasks.FirstOrDefault(c => c.Id == "card1");
        Assert.IsNotNull(task);
        Assert.AreEqual("Forest Foraging", task.Title);
        Assert.AreEqual(60, task.DurationSeconds);
        Assert.AreEqual(10, task.RewardValue);
    }

    [TestMethod]
    public void ResourceManager_RetrievesCharacterData_Correctly()
    {
        // Assert
        var character = _resourceManager!.Characters.FirstOrDefault(c => c.Name == "Hero");
        Assert.IsNotNull(character);
        Assert.AreEqual("Squire", character.JobName);
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
