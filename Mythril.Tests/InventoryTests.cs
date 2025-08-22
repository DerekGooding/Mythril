using Mythril.Data;
using Mythril.Data.Items;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager? resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var items = new List<Item>
        {
            new ConsumableItem { Name = "Potion", Description = "Restores HP" },
            new EquipmentItem { Name = "Bronze Sword", Description = "A basic sword", Slot = EquipmentSlot.Weapon }
        };
        resourceManager.SetData([], [], [], [], items, []);
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        // Arrange
        var inventoryManager = resourceManager?.Inventory;
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
        var inventoryManager = resourceManager?.Inventory;
        Assert.IsNotNull(inventoryManager);

        // Act
        inventoryManager.Add("Potion", 5);

        // Assert
        Assert.IsTrue(inventoryManager.Has("Potion", 5));
        Assert.IsFalse(inventoryManager.Has("Potion", 6));
        Assert.IsFalse(inventoryManager.Has("Bronze Sword"));
    }
}
