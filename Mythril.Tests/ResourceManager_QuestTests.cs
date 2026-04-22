using Mythril.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class ResourceManager_QuestTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_IsInProgress_Works()
    {
        var character = _resourceManager!.Characters[0];
        var q1 = new QuestData(_quests!.All.First(q => q.Name == "Hunt Goblins"), _questDetails![_quests.All.First(q => q.Name == "Hunt Goblins")]);
        
        Assert.IsFalse(_resourceManager.IsInProgress(q1));
        
        _resourceManager.StartQuest(q1, character);
        Assert.IsTrue(_resourceManager.IsInProgress(q1));
    }

    [TestMethod]
    public void StartQuest_PreventsDuplicateInProgress()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Should not start duplicate single-use quest.");
    }

    [TestMethod]
    public void StartQuest_PreventsCompletedSingleUse()
    {
        var character = _resourceManager!.Characters[0];
        var prologue = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        
        _resourceManager.ReceiveRewards(prologue).Wait();
        Assert.IsTrue(_resourceManager.GetCompletedQuests().Contains(prologue.Quest));
        
        _resourceManager.StartQuest(prologue, character);
        Assert.AreEqual(0, _resourceManager.ActiveQuests.Count, "Should not start completed single-use quest.");
    }

    [TestMethod]
    public void StartQuest_CadenceUnlock_Works()
    {
        var character = _resourceManager!.Characters[0];
        var arcanist = _cadences!.All.First(c => c.Name == "Arcanist");
        var unlock = arcanist.Abilities.First(a => a.Ability.Name == "Refine Ice");
        
        // Find requirements for Refine Ice in Arcanist
        foreach(var req in unlock.Requirements) _resourceManager.Inventory.Add(req.Item, req.Quantity);
        
        _resourceManager.StartQuest(unlock, character);
        
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        Assert.IsTrue(_resourceManager.ActiveQuests[0].Item is CadenceUnlock);
    }
}
