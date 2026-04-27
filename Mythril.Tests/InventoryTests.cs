namespace Mythril.Tests;

[TestClass]
public class InventoryTests : ResourceManagerTestBase
{
    [TestMethod]
    public void ResourceManager_Inventory_IsAccessible() => Assert.IsNotNull(_resourceManager?.Inventory);

    [TestMethod]
    public void Inventory_AddAndRemove_Works()
    {
        var item = _items!.All.First(i => i.Name == "Gold");
        _resourceManager!.Inventory.Add(item, 10);
        Assert.AreEqual(10, _resourceManager.Inventory.GetQuantity(item));

        _resourceManager.Inventory.Remove(item, 4);
        Assert.AreEqual(6, _resourceManager.Inventory.GetQuantity(item));
    }

    [TestMethod]
    public void Inventory_Has_Works()
    {
        var item = _items!.All.First(i => i.Name == "Gold");
        _resourceManager!.Inventory.Add(item, 5);
        Assert.IsTrue(_resourceManager.Inventory.Has(item, 3));
        Assert.IsFalse(_resourceManager.Inventory.Has(item, 6));
    }

    [TestMethod]
    public void Inventory_GetAll_ReturnsCorrectItems()
    {
        var item1 = _items!.All[0];
        var item2 = _items!.All[1];
        _resourceManager!.Inventory.Clear();
        _resourceManager.Inventory.Add(item1, 5);
        _resourceManager.Inventory.Add(item2, 10);

        var all = _resourceManager.Inventory.GetAll().ToList();
        Assert.HasCount(2, all);
    }
}