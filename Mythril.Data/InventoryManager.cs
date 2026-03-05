namespace Mythril.Data;

public class InventoryManager
{
    private readonly Dictionary<Item, int> _inventory = [];
    public int MagicCapacity { get; set; } = 30;

    private readonly HashSet<string> _pinnedItemNames = [];

    public void TogglePin(string itemName)
    {
        if (!_pinnedItemNames.Add(itemName))
            _pinnedItemNames.Remove(itemName);
    }

    public bool IsPinned(string itemName) => _pinnedItemNames.Contains(itemName);

    public IEnumerable<ItemQuantity> GetPinnedItems()
        => _inventory.Where(x => _pinnedItemNames.Contains(x.Key.Name))
                     .Select(x => new ItemQuantity(x.Key, x.Value));

    public int Add(Item item, int quantity = 1)
    {
        if (quantity <= 0) return 0;
        
        int current = _inventory.GetValueOrDefault(item);
        int next = current + quantity;
        int overflow = 0;

        if (item.ItemType == ItemType.Spell && next > MagicCapacity)
        {
            overflow = next - MagicCapacity;
            next = MagicCapacity;
        }

        _inventory[item] = next;
        return overflow;
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

    public IEnumerable<ItemQuantity> GetAll()
        => _inventory.Select(x => new ItemQuantity(x.Key, x.Value));

    public IEnumerable<ItemQuantity> GetItems()
        => _inventory.Where(x => x.Key.ItemType != ItemType.Spell)
                     .Select(x => new ItemQuantity(x.Key, x.Value));
    public IEnumerable<ItemQuantity> GetSpells()
        => _inventory.Where(x => x.Key.ItemType == ItemType.Spell)
                     .Select(x => new ItemQuantity(x.Key, x.Value));
}
