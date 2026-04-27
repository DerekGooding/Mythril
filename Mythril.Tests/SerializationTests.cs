using Mythril.Data;
using System.Text.Json;

namespace Mythril.Tests;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void SaveData_RoundTrip_SystemTextJson()
    {
        var saveData = new SaveData
        {
            Inventory = new Dictionary<string, int> { { "Gold", 100 } },
            MagicCapacity = 50,
            PinnedItems = ["Potion"],
            LastSaveTime = DateTime.Now
        };

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var json = JsonSerializer.Serialize(saveData, options);
        var deserialized = JsonSerializer.Deserialize<SaveData>(json, options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(100, deserialized.Inventory["Gold"]);
        Assert.AreEqual(50, deserialized.MagicCapacity);
        Assert.AreEqual("Potion", deserialized.PinnedItems[0]);
    }

    [TestMethod]
    public void QuestProgressDTO_RoundTrip()
    {
        var dto = new QuestProgressDTO
        {
            ItemName = "Test Quest",
            ItemType = "Quest",
            CharacterName = "Hero",
            DurationSeconds = 10,
            Description = "Desc",
            StartTime = DateTime.Now,
            SecondsElapsed = 5.5
        };

        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<QuestProgressDTO>(json);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(dto.ItemName, deserialized.ItemName);
        Assert.AreEqual(dto.SecondsElapsed, deserialized.SecondsElapsed);
    }
}