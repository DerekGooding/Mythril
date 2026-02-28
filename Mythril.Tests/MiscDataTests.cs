using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class MiscDataTests
{
    [TestMethod]
    public void StatAugments_ReturnsCorrectValue()
    {
        var items = ContentHost.GetContent<Items>();
        var statAugments = ContentHost.GetContent<StatAugments>();
        var stats = ContentHost.GetContent<Stats>();

        var result = statAugments[items.FireI];
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(stats.Strength, result[0].Stat);
        Assert.AreEqual(20, result[0].ModifierAtFull);
    }

    [TestMethod]
    public void AbilityAugments_ReturnsCorrectValue()
    {
        var abilities = ContentHost.GetContent<CadenceAbilities>();
        var abilityAugments = ContentHost.GetContent<AbilityAugments>();
        var stats = ContentHost.GetContent<Stats>();

        var result = abilityAugments[abilities.AugmentStrength];
        Assert.AreEqual(stats.Strength, result);
    }

    [TestMethod]
    public void Locations_All_ContainsCorrectLocations()
    {
        var locations = ContentHost.GetContent<Locations>();
        Assert.AreEqual(5, locations.All.Length);
        Assert.IsTrue(locations.All.Any(l => l.Name == "Village"));
    }

    [TestMethod]
    public void Location_Name_ReturnsCorrectValue()
    {
        var quests = ContentHost.GetContent<Quests>();
        var location = new Location("Test", [quests.Prologue]);
        Assert.AreEqual("Test", location.Name);
        Assert.AreEqual(1, location.Quests.Count());
    }

    [TestMethod]
    public void ItemQuantity_Properties_ReturnCorrectValues()
    {
        var items = ContentHost.GetContent<Items>();
        var iq = new ItemQuantity(items.Gold, 100);
        Assert.AreEqual(items.Gold, iq.Item);
        Assert.AreEqual(100, iq.Quantity);
    }
}
