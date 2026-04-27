using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class JournalTests : ResourceManagerTestBase
{
    [TestMethod]
    public async Task Journal_AddEntry_Works()
    {
        var quest = ContentHost.GetContent<Quests>().All.First();
        var detail = ContentHost.GetContent<QuestDetails>()[quest];
        var questData = new QuestData(quest, detail);
        var character = _resourceManager!.Characters[0];

        _resourceManager.StartQuest(questData, character);
        var progress = _resourceManager.ActiveQuests[0];
        
        await _resourceManager.ReceiveRewards(progress);

        Assert.AreEqual(1, _resourceManager.Journal.Count);
        Assert.AreEqual(quest.Name, _resourceManager.Journal[0].TaskName);
    }

    [TestMethod]
    public void Journal_Clear_Works()
    {
        _resourceManager!.AddToJournal("Test", "Hero", "Details");
        Assert.AreEqual(1, _resourceManager.Journal.Count);

        _resourceManager.ClearJournal();
        Assert.AreEqual(0, _resourceManager.Journal.Count);
    }

}
