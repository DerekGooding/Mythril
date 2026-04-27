using Mythril.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class RewardTests : BunitTestBase
{
    [TestMethod]
    public async Task ReceiveRewards_Quest_AddsItemsToInventory()
    {
        // Arrange
        var item = new Item("Stone", "Desc", ItemType.Material);
        var quest = new Quest("Test Quest", "Desc");
        var detail = new QuestDetail(10, [], [new ItemQuantity(item, 5)], QuestType.Single);
        var questData = new QuestData(quest, detail);
        var character = new Character("Hero");

        // Start it so it's in ActiveQuests (needed for character name in journal)
        ResourceManager.StartQuest(questData, character);
        var progress = ResourceManager.ActiveQuests.First();

        // Act
        await ResourceManager.ReceiveRewards(progress);

        // Assert
        Assert.AreEqual(5, InventoryManager.GetQuantity(item));
    }

    [TestMethod]
    public async Task ReceiveRewards_CadenceUnlock_AddsAbility()
    {
        // Arrange
        var ability = new CadenceAbility("Fire Ball", "Desc");
        var unlock = new CadenceUnlock("Mage", ability, []);
        var character = new Character("Hero");

        ResourceManager.StartQuest(unlock, character);
        var progress = ResourceManager.ActiveQuests.First();

        // Act
        await ResourceManager.ReceiveRewards(progress);

        // Assert
        Assert.IsTrue(ResourceManager.UnlockedAbilities.Contains("Mage:Fire Ball"));
    }

    [TestMethod]
    public async Task ReceiveRewards_Refinement_AddsOutput()
    {
        // Arrange
        var ability = new CadenceAbility("Refine", "Desc");
        var input = new Item("Iron", "Desc", ItemType.Material);
        var output = new Item("Steel", "Desc", ItemType.Material);
        var recipe = new Recipe(1, output, 2);
        var refinement = new RefinementData(ability, input, recipe);
        var character = ResourceManager.Characters[0];
        ResourceManager.IsTestMode = true;

        // Ensure character HAS the ability and inventory HAS the input
        var cadence = new Cadence("Refiner", "Desc", [new CadenceUnlock("Refiner", ability, [], "Strength")]);
        var cadences = ContentHost.GetContent<Cadences>();
        cadences.Load(cadences.All.Concat([cadence]));
        
        ResourceManager.UnlockCadence(cadence);
        ResourceManager.UnlockAbility("Refiner", "Refine");
        JunctionManager.AssignCadence(cadence, character, ResourceManager.UnlockedAbilities);
        InventoryManager.Add(input, 1);

        ResourceManager.StartQuest(refinement, character);
        var progress = ResourceManager.ActiveQuests.First();

        // Act
        await ResourceManager.ReceiveRewards(progress);

        // Assert
        Assert.AreEqual(2, InventoryManager.GetQuantity(output));
    }

    [TestMethod]
    public void RestoreCompletedQuest_UnlocksQuestAndCadences()
    {
        // Arrange
        var quest = new Quest("Unlock Quest", "Desc");
        var quests = TestContext!.Services.GetRequiredService<Quests>();
        quests.Load(quests.All.Concat([quest]));
        
        var cadence = new Cadence("Secret Cadence", "Desc", []);
        Cadences.Load(Cadences.All.Concat([cadence]));
        
        // This is a bit internal, but we need to set up the mapping
        // In a real scenario, this is loaded from JSON
        var questToCadence = TestContext!.Services.GetRequiredService<QuestToCadenceUnlocks>();
        questToCadence.Load(new Dictionary<Quest, Cadence[]> { { quest, [cadence] } });

        // Act
        ResourceManager.RestoreCompletedQuest(quest);

        // Assert
        Assert.IsTrue(ResourceManager.GetCompletedQuests().Contains(quest));
        Assert.IsTrue(ResourceManager.UnlockedCadences.Any(c => c.Name == "Secret Cadence"));
    }
}
