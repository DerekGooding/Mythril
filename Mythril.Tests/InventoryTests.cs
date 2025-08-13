using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mythril.GameLogic;
using Mythril.GameLogic.Items;
using System.Linq;

namespace Mythril.Tests
{
    [TestClass]
    public class InventoryTests
    {
        [TestMethod]
        public void ResourceManager_LoadsItemData_Correctly()
        {
            // Act
            var resourceManager = new ResourceManager();

            // Assert
            Assert.IsNotNull(resourceManager.Items);
            Assert.IsTrue(resourceManager.Items.Count > 0);

            var potion = resourceManager.Items.FirstOrDefault(i => i.Name == "Potion") as ConsumableItem;
            Assert.IsNotNull(potion);
            Assert.AreEqual(ItemType.Consumable, potion.Type);
            Assert.AreEqual(50, potion.Value);

            var sword = resourceManager.Items.FirstOrDefault(i => i.Name == "Bronze Sword") as EquipmentItem;
            Assert.IsNotNull(sword);
            Assert.AreEqual(ItemType.Equipment, sword.Type);
            Assert.AreEqual(EquipmentSlot.Weapon, sword.Slot);
        }

        [TestMethod]
        public void InventoryManager_AddsAndRemovesItems_Correctly()
        {
            // Arrange
            var resourceManager = new ResourceManager();
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
}
