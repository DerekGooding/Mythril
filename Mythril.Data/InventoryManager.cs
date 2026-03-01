namespace Mythril.Data;

public class InventoryManager
{
    private readonly Dictionary<Item, int> _inventory = [];

    public void Add(Item item, int quantity = 1)
    {
        if (quantity <= 0) return;
        if (_inventory.ContainsKey(item))
            _inventory[item] += quantity;
        else
            _inventory[item] = quantity;
    }

    public bool Remove(Item item, int quantity = 1)
    {
        if (quantity <= 0) return true;
        if (!_inventory.TryGetValue(item, out var value) || value < quantity)
            return false;

        _inventory[item] -= quantity;
        if (_inventory[item] == 0 && item.Name != "Gold")
            _inventory.Remove(item);

        return true;
    }

    public bool Has(Item item, int quantity = 1) 
    {
        if (quantity <= 0) return true;
        return _inventory.ContainsKey(item) && _inventory[item] >= quantity;
    }

    public int GetQuantity(Item item) => _inventory.GetValueOrDefault(item);

    public void Clear() => _inventory.Clear();

    public IEnumerable<ItemQuantity> GetItems()
        => _inventory.Where(x => x.Key.ItemType != ItemType.Spell)
                     .Select(x => new ItemQuantity(x.Key, x.Value));
    public IEnumerable<ItemQuantity> GetSpells()
        => _inventory.Where(x => x.Key.ItemType == ItemType.Spell)
                     .Select(x => new ItemQuantity(x.Key, x.Value));
}
