using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Collections.Immutable;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class GameStoreTests
{
    [TestMethod]
    public void GameStore_InitialState_IsCorrect()
    {
        var store = new GameStore();
        Assert.AreEqual(0, store.State.Inventory.Count);
        Assert.AreEqual(30, store.State.MagicCapacity);
        Assert.AreEqual(0, store.State.CompletedQuests.Count);
    }

    [TestMethod]
    public void GameStore_AddResource_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new AddResourceAction("Potion", 5));
        
        Assert.AreEqual(5, store.State.Inventory["Potion"]);
    }

    [TestMethod]
    public void GameStore_SpendResource_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new AddResourceAction("Potion", 10));
        store.Dispatch(new SpendResourceAction("Potion", 4));
        
        Assert.AreEqual(6, store.State.Inventory["Potion"]);
    }

    [TestMethod]
    public void GameStore_CompleteQuest_UpdatesState()
    {
        var store = new GameStore();
        var quest = new Quest("Prologue", "Test");
        store.Dispatch(new CompleteQuestAction(quest));
        
        Assert.IsTrue(store.State.CompletedQuests.Contains("Prologue"));
    }

    [TestMethod]
    public void GameStore_Tick_UpdatesActiveQuests()
    {
        var store = new GameStore();
        var character = new Character("Test");
        var progress = new QuestProgress("item", "desc", 10, character, 0);
        
        store.Dispatch(new StartQuestAction(progress));
        store.Dispatch(new TickAction(5.0));
        
        Assert.AreEqual(5.0, store.State.ActiveQuests[0].SecondsElapsed);
        Assert.AreEqual(5.0, store.State.CurrentTime);
    }

    [TestMethod]
    public void GameStore_UnlockLocation_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new UnlockLocationAction("New Zone"));
        Assert.IsTrue(store.State.UnlockedLocationNames.Contains("New Zone"));
    }

    [TestMethod]
    public void GameStore_UnlockCadence_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new UnlockCadenceAction("Secret Job"));
        Assert.IsTrue(store.State.UnlockedCadenceNames.Contains("Secret Job"));
    }

    [TestMethod]
    public void GameStore_UnlockAbility_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new UnlockAbilityAction("Job:Skill"));
        Assert.IsTrue(store.State.UnlockedAbilities.Contains("Job:Skill"));
    }

    [TestMethod]
    public void GameStore_JunctionMagic_UpdatesState()
    {
        var store = new GameStore();
        var hero = new Character("Hero");
        var str = new Stat("Strength", "");
        var fire = new Item("Fire", "", ItemType.Spell);
        
        store.Dispatch(new JunctionMagicAction(hero, str, fire));
        Assert.AreEqual(1, store.State.Junctions.Count);
        Assert.AreEqual("Fire", store.State.Junctions[0].Magic.Name);
    }

    [TestMethod]
    public void GameStore_AssignCadence_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new AssignCadenceAction("Warrior", "Hero"));
        Assert.AreEqual("Hero", store.State.AssignedCadences["Warrior"]);
    }

    [TestMethod]
    public void GameStore_AddStatBoost_UpdatesState()
    {
        var store = new GameStore();
        store.Dispatch(new AddStatBoostAction("Hero", "Strength", 5));
        Assert.AreEqual(5, store.State.CharacterPermanentStatBoosts["Hero"]["Strength"]);
        
        store.Dispatch(new AddStatBoostAction("Hero", "Strength", 10));
        Assert.AreEqual(15, store.State.CharacterPermanentStatBoosts["Hero"]["Strength"]);
    }
}
