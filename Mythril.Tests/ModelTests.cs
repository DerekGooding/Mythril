using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ModelTests
{
    [TestMethod]
    public void DTO_Instantiation_Tests()
    {
        // Testing DTOs for coverage
        var itemQty = new ItemQuantityDTO { Item = "Test", Quantity = 5 };
        Assert.AreEqual("Test", itemQty.Item);
        Assert.AreEqual(5, itemQty.Quantity);

        var loc = new LocationDTO { Name = "Loc", Quests = ["Q1"] };
        Assert.AreEqual("Loc", loc.Name);
        Assert.AreEqual(1, loc.Quests.Count);

        var ability = new CadenceAbilityUnlockDTO { Ability = "Ab", Requirements = [itemQty] };
        Assert.AreEqual("Ab", ability.Ability);
        Assert.AreEqual(1, ability.Requirements.Count);

        var cadence = new CadenceDTO { Name = "Cad", Description = "Desc", Abilities = [ability] };
        Assert.AreEqual("Cad", cadence.Name);
        Assert.AreEqual(1, cadence.Abilities.Count);

        var detail = new QuestDetailDTO { Quest = "Q", DurationSeconds = 10, Type = "Single", Requirements = [], Rewards = [] };
        Assert.AreEqual("Q", detail.Quest);
        Assert.AreEqual(10, detail.DurationSeconds);

        var unlock = new QuestUnlockDTO { Quest = "Q2", Requires = ["Q1"] };
        Assert.AreEqual("Q2", unlock.Quest);
        Assert.AreEqual(1, unlock.Requires.Count);

        var questCadence = new QuestCadenceUnlockDTO { Quest = "Q", Cadences = ["C"] };
        Assert.AreEqual("Q", questCadence.Quest);
        Assert.AreEqual(1, questCadence.Cadences.Count);

        var recipe = new RecipeDTO { InputItem = "I", InputQuantity = 1, OutputItem = "O", OutputQuantity = 2 };
        Assert.AreEqual("I", recipe.InputItem);

        var refinement = new RefinementDTO { Ability = "A", Recipes = [recipe] };
        Assert.AreEqual("A", refinement.Ability);

        var statEntry = new StatAugmentEntryDTO { Stat = "S", ModifierAtFull = 10 };
        Assert.AreEqual("S", statEntry.Stat);

        var statItem = new StatAugmentItemDTO { Item = "I", Augments = [statEntry] };
        Assert.AreEqual("I", statItem.Item);

        var save = new SaveData { Inventory = [], UnlockedCadences = [], UnlockedAbilities = [], CompletedQuests = [], ActiveQuests = [], Junctions = [], AssignedCadences = [], LastSaveTime = DateTime.Now };
        Assert.IsNotNull(save.Inventory);

        var assigned = new AssignedCadenceDTO { CharacterName = "C", CadenceName = "Cad" };
        Assert.AreEqual("C", assigned.CharacterName);

        var junction = new JunctionDTO { CharacterName = "C", StatName = "S", MagicName = "M" };
        Assert.AreEqual("C", junction.CharacterName);

        var progress = new QuestProgressDTO { ItemName = "I", ItemType = "T", CharacterName = "C", SecondsElapsed = 1, StartTime = DateTime.Now };
        Assert.AreEqual("I", progress.ItemName);
    }

    [TestMethod]
    public void QuestToCadenceUnlocks_Tests()
    {
        var content = new QuestToCadenceUnlocks();
        var q = new Quest("Q", "D");
        var c = new Cadence("C", "D", []);
        
        content.Load(new Dictionary<Quest, Cadence[]> { { q, [c] } });
        Assert.AreEqual(1, content[q].Length);
        Assert.AreEqual(0, content[new Quest("X", "X")].Length);
    }

    [TestMethod]
    public void ItemRefinements_Tests()
    {
        var content = new ItemRefinements();
        var a = new CadenceAbility("A", "D");
        var item = new Item("I", "D", ItemType.Material);
        var recipe = new Recipe(1, item, 1);
        
        content.Load(new Dictionary<CadenceAbility, Dictionary<Item, Recipe>> { { a, new Dictionary<Item, Recipe> { { item, recipe } } } });
        Assert.AreEqual(1, content[a].Count);
        Assert.AreEqual(0, content[new CadenceAbility("X", "X")].Count);
    }

    [TestMethod]
    public void RefinementBuilder_Tests()
    {
        var item = new Item("I", "D", ItemType.Material);
        var recipe = new Recipe(1, item, 1);
        var dict = RefinementBuilder.Recipes(new KeyValuePair<Item, Recipe>(item, recipe));
        Assert.AreEqual(1, dict.Count);
    }
}
