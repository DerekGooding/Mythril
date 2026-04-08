using System.Collections.Generic;
using System.Linq;

namespace Mythril.Data;

public class InventoryManager(GameStore gameStore)
{
    private readonly GameStore _gameStore = gameStore;

    public int MagicCapacity
    {
        get => _gameStore.State.MagicCapacity;
        set => _gameStore.Dispatch(new SetMagicCapacityAction(value));
    }

    public void TogglePin(string itemName)
    {
        _gameStore.Dispatch(new TogglePinAction(itemName));
    }

    public bool IsPinned(string itemName) => _gameStore.State.PinnedItems.Contains(itemName);

    public IEnumerable<ItemQuantity> GetPinnedItems()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.PinnedItems
            .Select(name => items.All.FirstOrDefault(i => i.Name == name))
            .Where(i => i.Name != null)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory.GetValueOrDefault(i.Name)));
    }

    public int Add(Item item, int quantity = 1)
    {
        if (quantity <= 0) return 0;
        
        int current = _gameStore.State.Inventory.GetValueOrDefault(item.Name);
        int next = current + quantity;
        int overflow = 0;

        if (item.ItemType == ItemType.Spell && next > MagicCapacity)
        {
            overflow = next - MagicCapacity;
            next = MagicCapacity;
        }

        _gameStore.Dispatch(new AddResourceAction(item.Name, quantity - overflow));
        return overflow;
    }

    public bool Remove(Item item, int quantity = 1)
    {
        if (quantity <= 0) return true;
        if (!Has(item, quantity))
            return false;

        _gameStore.Dispatch(new SpendResourceAction(item.Name, quantity));
        return true;
    }

    public bool Has(Item item, int quantity = 1) 
    {
        if (quantity <= 0) return true;
        return _gameStore.State.Inventory.GetValueOrDefault(item.Name) >= quantity;
    }

    public int GetQuantity(Item item) => _gameStore.State.Inventory.GetValueOrDefault(item.Name);

    public void Clear()
    {
        // This is a bit heavy, maybe Add a ClearInventoryAction
        foreach (var item in _gameStore.State.Inventory)
        {
            _gameStore.Dispatch(new SpendResourceAction(item.Key, item.Value));
        }
    }

    public IEnumerable<ItemQuantity> GetAll()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => new ItemQuantity(items.All.First(i => i.Name == kv.Key), kv.Value));
    }

    public IEnumerable<ItemQuantity> GetItems()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => items.All.First(i => i.Name == kv.Key))
            .Where(i => i.ItemType != ItemType.Spell)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory[i.Name]));
    }

    public IEnumerable<ItemQuantity> GetSpells()
    {
        var items = ContentHost.GetContent<Items>();
        return _gameStore.State.Inventory
            .Select(kv => items.All.First(i => i.Name == kv.Key))
            .Where(i => i.ItemType == ItemType.Spell)
            .Select(i => new ItemQuantity(i, _gameStore.State.Inventory[i.Name]));
    }
}
