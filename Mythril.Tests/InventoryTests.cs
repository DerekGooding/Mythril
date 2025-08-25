using Mythril.Data;
using Mythril.Data.Items;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager? _resourceManager;

    [TestInitialize]
    public void Setup()
    {
        _resourceManager = new ResourceManager();
        var items = new List<Item>
        {
            new ConsumableItem { Name = "Potion", Description = "Restores HP" },
        };
        _resourceManager.SetData([], [], [], items);
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        // Arrange
        var inventoryManager = _resourceManager?.Inventory;
        Assert.IsNotNull(inventoryManager);

        // Act
        inventoryManager.Add("Potion", 5);
        inventoryManager.Add("Bronze Sword");

        // Assert
        Assert.AreEqual(5, inventoryManager.GetQuantity("Potion"));
        Assert.AreEqual(1, inventoryManager.GetQuantity("Bronze Sword"));

        // Act
        inventoryManager.Remove("Potion", 2);

        // Assert
        Assert.AreEqual(3, inventoryManager.GetQuantity("Potion"));
    }

    [TestMethod]
    public void InventoryManager_Has_Correctly()
    {
        // Arrange
        var inventoryManager = _resourceManager?.Inventory;
        Assert.IsNotNull(inventoryManager);

        // Act
        inventoryManager.Add("Potion", 5);

        // Assert
        Assert.IsTrue(inventoryManager.Has("Potion", 5));
        Assert.IsFalse(inventoryManager.Has("Potion", 6));
        Assert.IsFalse(inventoryManager.Has("Bronze Sword"));
    }
}
