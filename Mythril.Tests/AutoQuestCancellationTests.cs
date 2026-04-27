using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class AutoQuestCancellationTests : ResourceManagerTestBase
{
    [TestMethod]
    public void CancelledQuest_DoesNotAutoRestart_EvenIfAutoQuestEnabled()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        var recurringQuest = _quests!.All.First(q => q.Name == SandboxContent.BuyPotion);
        var questData = new QuestData(recurringQuest, _questDetails![recurringQuest]);

        // Add Gold for requirements
        _resourceManager.Inventory.Add(_items!.All.First(i => i.Name == SandboxContent.Gold), 100);

        // Enable AutoQuest (mocking the ability unlock)
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Act: Start and then Cancel the quest
        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests.First(p => p.Character.Name == character.Name);
        _resourceManager.CancelQuest(progress);

        // Simulate a tick to trigger CheckAutoQuestTick
        _resourceManager.Tick(1.0);

        // Assert
        var activeAfterTick = _resourceManager.ActiveQuests.Where(p => p.Character.Name == character.Name).ToList();
        Assert.IsEmpty(activeAfterTick, "Quest should not have auto-restarted after cancellation.");
    }

    [TestMethod]
    public async Task CompletedQuest_DoesAutoRestart_WhenAutoQuestEnabled()
    {
        // Arrange
        var character = _resourceManager!.Characters[0];
        var recurringQuest = _quests!.All.First(q => q.Name == SandboxContent.BuyPotion);
        var questData = new QuestData(recurringQuest, _questDetails![recurringQuest]);

        // Add Gold for requirements
        _resourceManager.Inventory.Add(_items!.All.First(i => i.Name == SandboxContent.Gold), 200);

        // Enable AutoQuest
        var recruit = _cadences!.All.First(c => c.Name == SandboxContent.Recruit);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockAbility(SandboxContent.Recruit, SandboxContent.AutoQuestI);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Act: Start and complete the quest
        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests.First(p => p.Character.Name == character.Name);

        // Complete the quest by ticking
        _resourceManager.Tick(questData.DurationSeconds + 1);

        // Simulation usually calls ReceiveRewards which finishes the quest
        await _resourceManager.ReceiveRewards(progress);

        // Simulate another tick to trigger CheckAutoQuestTick
        _resourceManager.Tick(1.0);

        // Assert
        var activeAfterTick = _resourceManager.ActiveQuests.Where(p => p.Character.Name == character.Name).ToList();
        Assert.IsNotEmpty(activeAfterTick, "Quest should have auto-restarted after completion.");
        Assert.AreEqual(recurringQuest.Name, activeAfterTick[0].Name);
    }
}