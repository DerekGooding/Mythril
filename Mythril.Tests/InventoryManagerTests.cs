using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class InventoryManagerTests
{
    private Items? _items;
    private InventoryManager? _inventoryManager;
    private GameStore? _gameStore;

    [TestInitialize]
    public void Setup()
    {
        SandboxContent.Load();
        _items = ContentHost.GetContent<Items>();
        _gameStore = new GameStore();
        _inventoryManager = new InventoryManager(_gameStore);
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        var basicGem = _items!.All.First(x => x.Name == SandboxContent.BasicGem);

        // Act
        _inventoryManager!.Add(potion, 5);
        _inventoryManager.Add(basicGem);

        // Assert
        Assert.AreEqual(5, _inventoryManager.GetQuantity(potion));
        Assert.AreEqual(1, _inventoryManager.GetQuantity(basicGem));

        // Act
        var result = _inventoryManager.Remove(potion, 2);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(3, _inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_FailsWhenInsufficient()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);

        _inventoryManager!.Add(potion, 5);
        var result = _inventoryManager.Remove(potion, 10);

        Assert.IsFalse(result);
        Assert.AreEqual(5, _inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_RemovesFromDictionaryWhenZero()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);

        _inventoryManager!.Add(potion, 5);
        _inventoryManager.Remove(potion, 5);

        Assert.AreEqual(0, _inventoryManager.GetQuantity(potion));
        Assert.DoesNotContain(i => i.Item == potion, _inventoryManager.GetItems());
    }

    [TestMethod]
    public void InventoryManager_Remove_DoesNotRemoveGoldWhenZero()
    {
        var gold = _items!.All.First(x => x.Name == SandboxContent.Gold);

        _inventoryManager!.Add(gold, 5);
        _inventoryManager.Remove(gold, 5);

        Assert.AreEqual(0, _inventoryManager.GetQuantity(gold));
    }

    [TestMethod]
    public void InventoryManager_Has_Correctly()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        var basicGem = _items!.All.First(x => x.Name == SandboxContent.BasicGem);

        // Act
        _inventoryManager!.Add(potion, 5);

        // Assert
        Assert.IsTrue(_inventoryManager.Has(potion, 5));
        Assert.IsFalse(_inventoryManager.Has(potion, 6));
        Assert.IsFalse(_inventoryManager.Has(basicGem));
    }

    [TestMethod]
    public void InventoryManager_GetItemsAndSpells_FilterCorrectly()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        var fire = _items!.All.First(x => x.Name == SandboxContent.FireI);

        _inventoryManager!.Add(potion, 1);
        _inventoryManager.Add(fire, 1);

        var items = _inventoryManager.GetItems().ToList();
        var spells = _inventoryManager.GetSpells().ToList();

        Assert.Contains(i => i.Item == potion, items);
        Assert.DoesNotContain(i => i.Item == fire, items);
        Assert.Contains(i => i.Item == fire, spells);
        Assert.DoesNotContain(i => i.Item == potion, spells);
    }

    [TestMethod]
    public void InventoryManager_GetQuantity_ReturnsZeroForMissingItem()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        Assert.AreEqual(0, _inventoryManager!.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Has_ReturnsTrueForZeroQuantity()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        Assert.IsTrue(_inventoryManager!.Has(potion, 0));
    }

    [TestMethod]
    public void InventoryManager_Add_DoesNothingWithZeroQuantity()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        _inventoryManager!.Add(potion, 0);
        Assert.AreEqual(0, _inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Add_NegativeQuantity_DoesNothing()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        _inventoryManager!.Add(potion, -10);
        Assert.AreEqual(0, _inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_NegativeQuantity_ReturnsTrueAndDoesNothing()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);
        _inventoryManager!.Add(potion, 5);
        var result = _inventoryManager.Remove(potion, -5);
        Assert.IsTrue(result);
        Assert.AreEqual(5, _inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_MagicCapacity_Enforcement()
    {
        var fire = _items!.All.First(x => x.Name == SandboxContent.FireI);
        _gameStore!.Dispatch(new SetMagicCapacityAction(30));

        // Try adding 100
        _inventoryManager!.Add(fire, 100);

        Assert.AreEqual(30, _inventoryManager.GetQuantity(fire), "Should be capped at capacity.");
    }

    [TestMethod]
    public void InventoryManager_Pinning_Works()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);

        Assert.IsFalse(_inventoryManager!.IsPinned(SandboxContent.Potion));

        _inventoryManager.TogglePin(SandboxContent.Potion);
        Assert.IsTrue(_inventoryManager.IsPinned(SandboxContent.Potion));

        _inventoryManager.Add(potion, 5);
        var pinned = _inventoryManager.GetPinnedItems().ToList();
        Assert.HasCount(1, pinned);
        Assert.AreEqual(SandboxContent.Potion, pinned[0].Item.Name);
        Assert.AreEqual(5, pinned[0].Quantity);

        _inventoryManager.TogglePin(SandboxContent.Potion);
        Assert.IsFalse(_inventoryManager.IsPinned(SandboxContent.Potion));
        Assert.AreEqual(0, _inventoryManager.GetPinnedItems().Count());
    }

    [TestMethod]
    public void InventoryManager_Clear_Works()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);

        _inventoryManager!.Add(potion, 5);
        Assert.AreEqual(5, _inventoryManager.GetQuantity(potion));

        _inventoryManager.Clear();
        Assert.AreEqual(0, _inventoryManager.GetQuantity(potion));
        Assert.AreEqual(0, _inventoryManager.GetAll().Count());
    }

    [TestMethod]
    public void InventoryManager_Subtract_Works()
    {
        var potion = _items!.All.First(x => x.Name == SandboxContent.Potion);

        _inventoryManager!.Add(potion, 10);
        _inventoryManager.Remove(potion, 4);
        Assert.AreEqual(6, _inventoryManager.GetQuantity(potion));

        var result = _inventoryManager.Remove(potion, 10); // Should return false
        Assert.IsFalse(result);
        Assert.AreEqual(6, _inventoryManager.GetQuantity(potion));
    }
}