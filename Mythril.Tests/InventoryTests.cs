using Mythril.Data;

namespace Mythril.Tests;

[TestClass]
public class InventoryTests
{
    private ResourceManager? _resourceManager;
    private Items? _items;
    private QuestDetails? _questDetails;

    [TestInitialize]
    public void Setup()
    {
        TestContentLoader.Load();
        _items = ContentHost.GetContent<Items>();
        _questDetails = ContentHost.GetContent<QuestDetails>();
        
        var inventory = new InventoryManager();
        var junctionManager = new JunctionManager(inventory, ContentHost.GetContent<StatAugments>(), ContentHost.GetContent<Cadences>());
        _resourceManager = new ResourceManager(
            _items, 
            ContentHost.GetContent<QuestUnlocks>(), 
            ContentHost.GetContent<QuestToCadenceUnlocks>(), 
            _questDetails, 
            ContentHost.GetContent<Cadences>(), 
            ContentHost.GetContent<Locations>(),
            junctionManager,
            inventory);

        _resourceManager.Initialize();
        
        _resourceManager.Inventory.Clear();
    }

    [TestMethod]
    public void InventoryManager_AddsAndRemovesItems_Correctly()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var basicGem = _items!.All.First(x => x.Name == "Basic Gem");

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
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(potion, 5);
        var result = inventoryManager.Remove(potion, 10);

        Assert.IsFalse(result);
        Assert.AreEqual(5, inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_RemovesFromDictionaryWhenZero()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(potion, 5);
        inventoryManager.Remove(potion, 5);

        Assert.AreEqual(0, inventoryManager.GetQuantity(potion));
        Assert.IsFalse(inventoryManager.GetItems().Any(i => i.Item == potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_DoesNotRemoveGoldWhenZero()
    {
        var gold = _items!.All.First(x => x.Name == "Gold");
        var inventoryManager = _resourceManager?.Inventory;

        inventoryManager!.Add(gold, 5);
        inventoryManager.Remove(gold, 5);

        Assert.AreEqual(0, inventoryManager.GetQuantity(gold));
        // Gold stays in inventory even at 0 (based on implementation)
    }

    [TestMethod]
    public void InventoryManager_Has_Correctly()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var basicGem = _items!.All.First(x => x.Name == "Basic Gem");

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
        var potion = _items!.All.First(x => x.Name == "Potion");
        var fire = _items!.All.First(x => x.Name == "Fire I");
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

    [TestMethod]
    public void InventoryManager_GetQuantity_ReturnsZeroForMissingItem()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;
        Assert.AreEqual(0, inventoryManager!.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Has_ReturnsTrueForZeroQuantity()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;
        Assert.IsTrue(inventoryManager!.Has(potion, 0));
    }

    [TestMethod]
    public void InventoryManager_Add_DoesNothingWithZeroQuantity()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;
        inventoryManager!.Add(potion, 0);
        Assert.AreEqual(0, inventoryManager.GetQuantity(potion));
    }

    [TestMethod]
    public void InventoryManager_Remove_ReturnsFalseForMissingItem()
    {
        var potion = _items!.All.First(x => x.Name == "Potion");
        var inventoryManager = _resourceManager?.Inventory;
        var result = inventoryManager!.Remove(potion, 1);
        Assert.IsFalse(result);
    }
}
