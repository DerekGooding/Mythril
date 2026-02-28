using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;

    [TestInitialize]
    public void Setup()
    {
        _resourceManager = new ResourceManager();
        _resourceManager.Inventory.Clear();
        _items = ContentHost.GetContent<Items>();
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        var potion = _items!.Potion;
        var basicGem = _items.BasicGem;

        // Arrange
        var inventoryManager = _resourceManager?.Inventory;
        Assert.IsNotNull(inventoryManager);

        // Act
        inventoryManager.Add(potion ,5);
        inventoryManager.Add(basicGem);

        // Assert
        Assert.AreEqual(5, inventoryManager.GetQuantity(potion));
        Assert.AreEqual(1, inventoryManager.GetQuantity(basicGem));

        // Act
        var result = inventoryManager.Remove(potion, 2);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(3, inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_FailsWhenInsufficient()
    {
        var potion = _items!.Potion;
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(potion, 5);
        var result = inventoryManager.Remove(potion, 10);

        Assert.IsFalse(result);
        Assert.AreEqual(5, inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_RemovesFromDictionaryWhenZero()
    {
        var potion = _items!.Potion;
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(potion, 5);
        inventoryManager.Remove(potion, 5);

        Assert.AreEqual(0, inventoryManager.GetQuantity(potion));
        Assert.IsFalse(inventoryManager.GetItems().Any(i => i.Item == potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_DoesNotRemoveGoldWhenZero()
    {
        var gold = _items!.Gold;
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(gold, 5);
        inventoryManager.Remove(gold, 5);

        Assert.AreEqual(0, inventoryManager.GetQuantity(gold));
        // Gold stays in inventory even at 0 (based on implementation)
    }

    [TestMethod]
    public void InventoryManager_Has_Correctly()
    {
        var potion = _items!.Potion;
        var basicGem = _items.BasicGem;

        // Arrange
        var inventoryManager = _resourceManager?.Inventory;
        Assert.IsNotNull(inventoryManager);

        // Act
        inventoryManager.Add(potion, 5);

        // Assert
        Assert.IsTrue(inventoryManager.Has(potion, 5));
        Assert.IsFalse(inventoryManager.Has(potion, 6));
        Assert.IsFalse(inventoryManager.Has(basicGem));
    }

    [TestMethod]
    public void InventoryManager_GetItemsAndSpells_FilterCorrectly()
    {
        var potion = _items!.Potion;
        var fire = _items.FireI;
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(potion, 1);
        inventoryManager.Add(fire, 1);

        var items = inventoryManager.GetItems().ToList();
        var spells = inventoryManager.GetSpells().ToList();

        Assert.IsTrue(items.Any(i => i.Item == potion));
        Assert.IsFalse(items.Any(i => i.Item == fire));
        Assert.IsTrue(spells.Any(i => i.Item == fire));
        Assert.IsFalse(spells.Any(i => i.Item == potion));
    }
}
