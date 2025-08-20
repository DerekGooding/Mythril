using Mythril.Data.Items;
using Mythril.GameLogic;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager resourceManager;

    [TestInitialize]
    public void Setup()
    {
        resourceManager = new ResourceManager();
        var items = new List<Item>
        {
            new ConsumableItem("Potion", "Restores HP", 50),
            new EquipmentItem("Bronze Sword", "A basic sword", 100, EquipmentSlot.Weapon)
        };
        resourceManager.SetData([], [], [], [], items, []);
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        // Arrange
        var inventoryManager = resourceManager.Inventory;

        // Act
        inventoryManager.AddItem("Potion", 5);
        inventoryManager.AddItem("Bronze Sword");

        // Assert
        Assert.AreEqual(5, inventoryManager.GetItemCount("Potion"));
        Assert.AreEqual(1, inventoryManager.GetItemCount("Bronze Sword"));

        // Act
        inventoryManager.RemoveItem("Potion", 2);

        // Assert
        Assert.AreEqual(3, inventoryManager.GetItemCount("Potion"));
    }
}
