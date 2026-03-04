using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.Data;
using System.Linq;

namespace Mythril.Tests;

[TestClass]
public class LogisticsTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private Quests? _quests;
    private QuestDetails? _questDetails;
    private Cadences? _cadences;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _quests = ContentHost.GetContent<Quests>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        _cadences = ContentHost.GetContent<Cadences>();
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), _cadences);
        _resourceManager = new ResourceManager(
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            _cadences, 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory,
            ContentHost.GetContent<ItemRefinements>());
        _resourceManager.Initialize();
    }

    [TestMethod]
    public void Character_DefaultTaskLimit_IsOne()
    {
        var character = _resourceManager!.Characters[0];
        Assert.AreEqual(1, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_WithLogisticsI_TaskLimit_IsTwo()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        
        // Unlock and assign
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(2, _resourceManager.GetTaskLimit(character));
    }

    [TestMethod]
    public void Character_CannotExceedTaskLimit()
    {
        var character = _resourceManager!.Characters[0];
        var gold = _items!.All.First(x => x.Name == "Gold");
        _resourceManager.Inventory.Add(gold, 1000);

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        var quest2 = new QuestData(_quests.All.First(q => q.Name == "Visit Starting Town"), _questDetails[_quests.All.First(q => q.Name == "Visit Starting Town")]);

        // Limit is 1
        _resourceManager.StartQuest(quest1, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));

        _resourceManager.StartQuest(quest2, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name), "Should not allow second quest.");
    }

    [TestMethod]
    public void Character_WithLogisticsI_CanPerformTwoTasks()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        var gold = _items!.All.First(x => x.Name == "Gold");
        _resourceManager.Inventory.Add(gold, 1000);

        var quest1 = new QuestData(_quests!.All.First(q => q.Name == "Prologue"), _questDetails![_quests.All.First(q => q.Name == "Prologue")]);
        var quest2 = new QuestData(_quests.All.First(q => q.Name == "Visit Starting Town"), _questDetails[_quests.All.First(q => q.Name == "Visit Starting Town")]);

        _resourceManager.StartQuest(quest1, character);
        _resourceManager.StartQuest(quest2, character);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name), "Should allow two quests with Logistics I.");
    }

    [TestMethod]
    public void LosingLogisticsI_CancelsAndRefundsExcessTask()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);

        var gold = _items!.All.First(x => x.Name == "Gold");
        var iron = _items.All.First(x => x.Name == "Iron Ore");
        
        _resourceManager.Inventory.Clear();
        _resourceManager.Inventory.Add(gold, 1000);
        _resourceManager.Inventory.Add(iron, 100);

        // Quests with costs
        var quest1 = new QuestData(_quests!.All.First(q => q.Name == "Buy Potion"), _questDetails![_quests.All.First(q => q.Name == "Buy Potion")]); // Costs 250 gold
        var quest2 = new QuestData(_quests.All.First(q => q.Name == "Mine Iron Ore"), _questDetails[_quests.All.First(q => q.Name == "Mine Iron Ore")]); // Requirements check? actually it has no requirements in JSON cost-wise, but let's find one that does.
        
        // "Rekindling the Spark" costs Iron Ore
        var questSpark = new QuestData(_quests.All.First(q => q.Name == "Rekindling the Spark"), _questDetails[_quests.All.First(q => q.Name == "Rekindling the Spark")]); // Costs 10 Ancient Bark, 20 Iron Ore.
        // Let's add Ancient Bark
        var bark = _items.All.First(x => x.Name == "Ancient Bark");
        _resourceManager.Inventory.Add(bark, 10);

        _resourceManager.StartQuest(quest1, character);
        _resourceManager.StartQuest(questSpark, character);

        Assert.AreEqual(2, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));
        
        int goldAfterStart = _resourceManager.Inventory.GetQuantity(gold);
        int ironAfterStart = _resourceManager.Inventory.GetQuantity(iron);

        // Unequip weaver -> Lose Logistics I -> Should cancel questSpark (last added)
        _resourceManager.JunctionManager.Unassign(weaver, _resourceManager.UnlockedAbilities);

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count(q => q.Character.Name == character.Name));
        Assert.AreEqual("Buy Potion", _resourceManager.ActiveQuests[0].Name);

        // Check refund
        Assert.AreEqual(goldAfterStart, _resourceManager.Inventory.GetQuantity(gold), "First quest should NOT be refunded.");
        Assert.AreEqual(ironAfterStart + 20, _resourceManager.Inventory.GetQuantity(iron), "Second quest SHOULD be refunded.");
        Assert.AreEqual(10, _resourceManager.Inventory.GetQuantity(bark), "Ancient Bark SHOULD be refunded.");
    }

    [TestMethod]
    public void AutoQuest_OnlyRestartsSlotZero()
    {
        var character = _resourceManager!.Characters[0];
        var weaver = _cadences!.All.First(c => c.Name == "Mythril Weaver");
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        
        _resourceManager.UnlockCadence(weaver);
        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockedAbilities.Add("Mythril Weaver:Logistics I");
        _resourceManager.UnlockedAbilities.Add("Recruit:AutoQuest I");
        
        _resourceManager.JunctionManager.AssignCadence(weaver, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        var questGoblins = new QuestData(_quests!.All.First(q => q.Name == "Farm Goblins"), _questDetails![_quests.All.First(q => q.Name == "Farm Goblins")]);
        var questBats = new QuestData(_quests.All.First(q => q.Name == "Farm Bats"), _questDetails[_quests.All.First(q => q.Name == "Farm Bats")]);

        // Start both
        _resourceManager.StartQuest(questGoblins, character); // Slot 0
        _resourceManager.StartQuest(questBats, character);    // Slot 1

        var progress0 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 0);
        var progress1 = _resourceManager.ActiveQuests.First(p => p.SlotIndex == 1);

        // Simulate completion of slot 1
        _resourceManager.ReceiveRewards(progress1.Item).Wait();
        _resourceManager.RemoveActiveQuest(progress1);

        // Manual check of what Home.razor does:
        if (progress1.SlotIndex == 0 && progress1.Item is QuestData q1 && q1.Type == QuestType.Recurring)
        {
             _resourceManager.StartQuest(q1, character, -1.5);
        }

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Slot 1 should NOT have restarted.");
        Assert.AreEqual(0, _resourceManager.ActiveQuests.First().SlotIndex);

        // Simulate completion of slot 0
        _resourceManager.ReceiveRewards(progress0.Item).Wait();
        _resourceManager.RemoveActiveQuest(progress0);

        if (progress0.SlotIndex == 0 && progress0.Item is QuestData q0 && q0.Type == QuestType.Recurring)
        {
             _resourceManager.StartQuest(q0, character, -1.5);
        }

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Slot 0 SHOULD have restarted.");
        Assert.AreEqual(0, _resourceManager.ActiveQuests.First().SlotIndex);
    }

    [TestMethod]
    public void AutoQuest_RestartsRefinementsInSlotZero()
    {
        var character = _resourceManager!.Characters[0];
        var recruit = _cadences!.All.First(c => c.Name == "Recruit");
        var student = _cadences!.All.First(c => c.Name == "Student");
        var basicGem = _items!.All.First(i => i.Name == "Basic Gem");
        var ability = student.Abilities.First(a => a.Ability.Name == "Refine Fire").Ability;
        var recipe = ContentHost.GetContent<ItemRefinements>().ByKey[ability].Recipes[basicGem];
        var refinement = new RefinementData(ability, basicGem, recipe, "Magic");

        _resourceManager.UnlockCadence(recruit);
        _resourceManager.UnlockCadence(student);
        _resourceManager.UnlockedAbilities.Add("Recruit:AutoQuest I");
        _resourceManager.UnlockedAbilities.Add("Student:Refine Fire");
        
        _resourceManager.JunctionManager.AssignCadence(recruit, character, _resourceManager.UnlockedAbilities);
        _resourceManager.JunctionManager.AssignCadence(student, character, _resourceManager.UnlockedAbilities);
        _resourceManager.SetAutoQuestEnabled(character, true);

        // Add materials for multiple runs
        _resourceManager.Inventory.Add(basicGem, 10);

        // Start refinement in slot 0
        _resourceManager.StartQuest(refinement, character);
        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count);
        var progress = _resourceManager.ActiveQuests[0];
        Assert.AreEqual(0, progress.SlotIndex);

        // Simulate completion
        _resourceManager.ReceiveRewards(progress.Item).Wait();
        _resourceManager.RemoveActiveQuest(progress);

        // Logic from Home.razor.cs
        bool isRecurring = (progress.Item is QuestData q && q.Type == QuestType.Recurring) || 
                          (progress.Item is RefinementData);

        if (progress.SlotIndex == 0 && isRecurring)
        {
            if (_resourceManager.IsAutoQuestEnabled(progress.Character) && _resourceManager.CanAutoQuest(progress.Character))
            {
                if (_resourceManager.CanAfford(progress.Item, progress.Character))
                {
                    _resourceManager.StartQuest(progress.Item, progress.Character, -1.5);
                }
            }
        }

        Assert.AreEqual(1, _resourceManager.ActiveQuests.Count, "Refinement should have auto-restarted.");
        Assert.AreEqual(0, _resourceManager.ActiveQuests[0].SlotIndex);
    }
}
