using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class MiscDataTests
{
    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
    }

    [TestMethod]
    public void Quest_Equality_WorksCorrectly()
    {
        var q1 = new Quest("A", "DescA");
        var q2 = new Quest("B", "DescB");
        Assert.AreNotEqual(q1, q2, "Two different quests should NOT be equal.");
    }

    [TestMethod]
    public void StatAugments_ReturnsCorrectValue()
    {
        var items = ContentHost.GetContent<Items>();
        var statAugments = ContentHost.GetContent<StatAugments>();
        var stats = ContentHost.GetContent<Stats>();

        var potion = items.All.First(i => i.Name == "Potion");
        var augs = statAugments[potion];
        Assert.AreEqual(1, augs.Length);
        Assert.AreEqual("Health", augs[0].Stat.Name);
        Assert.AreEqual(10, augs[0].ModifierAtFull);
    }

    [TestMethod]
    public void AbilityAugments_ReturnsCorrectValue()
    {
        var abilities = ContentHost.GetContent<CadenceAbilities>();
        var abilityAugments = ContentHost.GetContent<AbilityAugments>();
        var stats = ContentHost.GetContent<Stats>();

        var autoQuest = abilities.All.First(a => a.Name == "AutoQuest I");
        var stat = abilityAugments[autoQuest];
        Assert.AreEqual("Magic", stat.Name);
    }

    [TestMethod]
    public void QuestProgress_Properties_ReturnCorrectValues()
    {
        var character = new Character("Hero");
        var quest = ContentHost.GetContent<Quests>().All.First();
        var detail = ContentHost.GetContent<QuestDetails>()[quest];
        var questData = new QuestData(quest, detail);

        var progress = new QuestProgress(questData, "Testing", 10, character);
        
        Assert.AreEqual(questData.Name, progress.Name);
        Assert.AreEqual("Testing", progress.Description);
        Assert.AreEqual(10, progress.DurationSeconds);
        Assert.AreEqual(character, progress.Character);
        Assert.AreEqual(0, progress.SecondsElapsed);
        Assert.AreEqual(0, progress.Progress);
        Assert.IsFalse(progress.IsCompleted);

        progress.SecondsElapsed = 5;
        Assert.AreEqual(0.5, progress.Progress);
        Assert.IsFalse(progress.IsCompleted);

        progress.SecondsElapsed = 10;
        Assert.AreEqual(1.0, progress.Progress);
        Assert.IsTrue(progress.IsCompleted);
    }

    [TestMethod]
    public void Locations_All_ContainsCorrectLocations()
    {
        var locations = ContentHost.GetContent<Locations>();
        Assert.AreEqual(6, locations.All.Length);
        Assert.IsTrue(locations.All.Any(l => l.Name == "Village"));
    }

    [TestMethod]
    public void Location_Name_ReturnsCorrectValue()
    {
        var quests = ContentHost.GetContent<Quests>();
        var location = new Location("Test", [quests.All.First()]);
        Assert.AreEqual("Test", location.Name);
        Assert.AreEqual(1, location.Quests.Count());
    }

    [TestMethod]
    public void QuestProgress_WithCadenceUnlock_Properties_ReturnCorrectValues()
    {
        var character = new Character("Hero");
        var cadence = ContentHost.GetContent<Cadences>().All.First();
        var unlock = cadence.Abilities[0];

        var progress = new QuestProgress(unlock, "Unlocking", 5, character);
        
        Assert.AreEqual(unlock.Ability.Name, progress.Name);
        Assert.AreEqual("Unlocking", progress.Description);
        Assert.AreEqual(5, progress.DurationSeconds);
        Assert.AreEqual(character, progress.Character);
    }

    [TestMethod]
    public void QuestData_RequirementsAndRewards_ReturnCorrectValues()
    {
        var quest = ContentHost.GetContent<Quests>().All.First(q => q.Name == "Buy Potion");
        var detail = ContentHost.GetContent<QuestDetails>()[quest];
        var questData = new QuestData(quest, detail);

        Assert.AreEqual(1, questData.Requirements.Length);
        Assert.AreEqual(1, questData.Rewards.Length);
        Assert.AreEqual("Gold", questData.Requirements[0].Item.Name);
        Assert.AreEqual("Potion", questData.Rewards[0].Item.Name);
    }

    [TestMethod]
    public void ItemQuantity_Properties_ReturnCorrectValues()
    {
        var items = ContentHost.GetContent<Items>();
        var iq = new ItemQuantity(items.All.First(), 100);
        Assert.AreEqual(items.All.First(), iq.Item);
        Assert.AreEqual(100, iq.Quantity);
    }
}
