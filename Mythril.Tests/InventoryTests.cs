using Mythril.Data;
using SimpleInjection.Injection;

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
        var host = Host.Initialize();
        _items = host.Get<Items>();
        //var items = new List<Item>
        //{
        //    new() { Name = "Potion", Description = "Restores HP" },
        //};
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
        inventoryManager.Remove(potion, 2);

        // Assert
        Assert.AreEqual(3, inventoryManager.GetQuantity(potion));
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
}
