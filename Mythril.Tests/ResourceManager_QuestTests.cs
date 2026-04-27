using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class ResourceManager_QuestTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_IsInProgress_Works()
    {
        var character = _resourceManager!.Characters[0];
        var q1 = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.HuntGoblins), _questDetails![_quests.All.First(q => q.Name == SandboxContent.HuntGoblins)]);

        Assert.IsFalse(_resourceManager.IsInProgress(q1));

        _resourceManager.StartQuest(q1, character);
        Assert.IsTrue(_resourceManager.IsInProgress(q1));
    }

    [TestMethod]
    public void StartQuest_PreventsDuplicateInProgress()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.Prologue), _questDetails![_quests.All.First(q => q.Name == SandboxContent.Prologue)]);

        _resourceManager.StartQuest(prologue, character);
        Assert.HasCount(1, _resourceManager.ActiveQuests);

        _resourceManager.StartQuest(prologue, character);
        Assert.HasCount(1, _resourceManager.ActiveQuests, "Should not start duplicate single-use quest.");
    }

    [TestMethod]
    public void StartQuest_PreventsCompletedSingleUse()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == SandboxContent.Prologue), _questDetails![_quests.All.First(q => q.Name == SandboxContent.Prologue)]);

        _resourceManager.ReceiveRewards(prologue).Wait(TestContext.CancellationToken);
        Assert.IsTrue(_resourceManager.GetCompletedQuests().Contains(prologue.Quest));

        _resourceManager.StartQuest(prologue, character);
        Assert.IsEmpty(_resourceManager.ActiveQuests, "Should not start completed single-use quest.");
    }

    [TestMethod]
    public void StartQuest_CadenceUnlock_Works()
    {
        var character = _resourceManager!.Characters[0];
        var arcanist = _cadences!.All.First(c => c.Name == SandboxContent.Arcanist);
        var ability = arcanist.Abilities.First(a => a.Ability.Name == SandboxContent.MagicPocketI).Ability;
        var unlock = new CadenceUnlock(SandboxContent.Arcanist, ability, [], SandboxContent.Magic);

        _resourceManager.StartQuest(unlock, character);

        Assert.HasCount(1, _resourceManager.ActiveQuests);
        Assert.IsTrue(_resourceManager.ActiveQuests[0].Item is CadenceUnlock);
    }

    public TestContext TestContext { get; set; }
}