using Mythril.Data;
using Mythril.Data.Materia;
using Mythril.GameLogic;

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
            new() { Id = "task1", Title = "Forest Foraging", DurationSeconds = 60, RewardValue = 10 }
        };
        var characters = new List<Character>
        {
            new("Hero", "Squire")
        };
        var materia = new List<Materia>
        {
            new MagicMateria("Fire", "Casts Fire spell", 100, 3, ["Fire1", "Fire2", "Fire3"]),
            new SummonMateria("Shiva", "Summons Shiva", 500, 5, "Shiva")
        };
        _resourceManager.SetData(tasks, characters, materia, [], [], []);
    }

    [TestMethod]
    public void ResourceManager_StoresAndRetrievesData_Correctly()
    {
        // Assert
        Assert.IsNotNull(_resourceManager!.Tasks);
        Assert.HasCount(1, _resourceManager.Tasks);
        Assert.IsNotNull(_resourceManager.Characters);
        Assert.HasCount(1, _resourceManager.Characters);
        Assert.IsNotNull(_resourceManager.Materia);
        Assert.HasCount(2, _resourceManager.Materia);
    }

    [TestMethod]
    public void ResourceManager_RetrievesTaskData_Correctly()
    {
        // Assert
        var task = _resourceManager!.Tasks.FirstOrDefault(c => c.Id == "task1");
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
    public void ResourceManager_RetrievesMateriaData_Correctly()
    {
        // Assert
        var fireMateria = _resourceManager!.Materia.FirstOrDefault(m => m.Name == "Fire") as MagicMateria;
        Assert.IsNotNull(fireMateria);
        Assert.AreEqual(MateriaType.Magic, fireMateria.Type);
        Assert.HasCount(3, fireMateria.Spells);

        var shivaMateria = _resourceManager.Materia.FirstOrDefault(m => m.Name == "Shiva") as SummonMateria;
        Assert.IsNotNull(shivaMateria);
        Assert.AreEqual(MateriaType.Summon, shivaMateria.Type);
        Assert.AreEqual("Shiva", shivaMateria.SummonName);
    }
}
