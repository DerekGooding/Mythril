using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class CompoundAlchemyTests
{
    [TestMethod]
    public void RefinementData_Description_FormatsMultipleInputsCorrectly()
    {
        var ability = new CadenceAbility("Refine Fire II", "Refine tier 2 fire magic");
        var inputItem = new Item("Fire I", "Tier 1 Fire Spell", ItemType.Spell);
        var extraInput = new Item("Iron Ore", "Iron Ore Material", ItemType.Material);
        var outputItem = new Item("Fire II", "Tier 2 Fire Spell", ItemType.Spell);

        var recipe = new Recipe(3, outputItem, 1, [new ItemQuantity(extraInput, 1)]);
        var refinement = new RefinementData(ability, inputItem, recipe, "Magic");

        Assert.IsTrue(refinement.Description.Contains("Refine 3x Fire I + 1x Iron Ore into 1x Fire II"));
    }
}
