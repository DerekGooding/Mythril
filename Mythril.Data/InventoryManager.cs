using System.Collections.Generic;
using System.Linq;

namespace Mythril.Data;

public class InventoryManager(GameStore gameStore)
{
    private readonly GameStore _gameStore = gameStore;

    public int MagicCapacity => _gameStore.State.MagicCapacity;

    public void TogglePin(string itemName) => _gameStore.Dispatch(new TogglePinAction(itemName));

    public bool IsPinned(string itemName) => _gameStore.State.PinnedItems.Contains(itemName);

    public IEnumerable<ItemQuantity> GetPinnedItems()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.PinnedItems
            .Select(name => items.All.FirstOrDefault(i => i.Name == name))
            .Where(i => i.Name != null)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory.GetValueOrDefault(i.Name!)));
    }

    public void Add(Item item, int quantity = 1)
    {
        if (quantity <= 0) return;
        _gameStore.Dispatch(new AddResourceAction(item.Name, quantity));
    }

    public bool Remove(Item item, int quantity = 1)
    {
        if (quantity <= 0) return true;
        if (!Has(item, quantity)) return false;

        _gameStore.Dispatch(new SpendResourceAction(item.Name, quantity));
        return true;
    }

    public bool Has(Item item, int quantity = 1) 
    {
        if (quantity <= 0) return true;
        return _gameStore.State.Inventory.GetValueOrDefault(item.Name) >= quantity;
    }

    public int GetQuantity(Item item) => _gameStore.State.Inventory.GetValueOrDefault(item.Name);

    public void Clear() => _gameStore.Dispatch(new ClearInventoryAction());

    public IEnumerable<ItemQuantity> GetAll()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => new { Item = items.All.FirstOrDefault(i => i.Name == kv.Key), Quantity = kv.Value })
            .Where(x => x.Item.Name != null)
            .Select(x => new ItemQuantity(x.Item, x.Quantity));
    }

    public IEnumerable<ItemQuantity> GetItems()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => items.All.FirstOrDefault(i => i.Name == kv.Key))
            .Where(i => i.Name != null && i.ItemType != ItemType.Spell)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory[i.Name!]));
    }

    public IEnumerable<ItemQuantity> GetSpells()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => items.All.FirstOrDefault(i => i.Name == kv.Key))
            .Where(i => i.Name != null && i.ItemType == ItemType.Spell)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory[i.Name!]));
    }
}
