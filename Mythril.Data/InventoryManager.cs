namespace Mythril.Data;

public class InventoryManager
{
    private readonly Dictionary<string, int> _resources = [];
    private readonly Item[] _items = ContentHost.GetContent<Items>().All;

    public void Add(string name, int quantity = 1)
    {
        if (_resources.ContainsKey(name))
            _resources[name] += quantity;
        else
            _resources[name] = quantity;
    }

    public bool Remove(string name, int quantity = 1)
    {
        if (!_resources.TryGetValue(name, out var value) || value < quantity)
            return false;

        _resources[name] -= quantity;
        if (_resources[name] == 0)
            _resources.Remove(name);

        return true;
    }

    public bool Has(string name, int quantity = 1) => _resources.ContainsKey(name) && _resources[name] >= quantity;

    public int GetQuantity(string name) => _resources.GetValueOrDefault(name);

    public void Clear() => _resources.Clear();

    public IEnumerable<Item> GetItems()
    {
        var items = new List<Item>();
        foreach (var resource in _resources)
        {
            if (_items.Any(i => i.Name == resource.Key))
            {
                var item = _items.FirstOrDefault(i => i.Name == resource.Key);
                item.Quantity = resource.Value;
                items.Add(item);
            }
        }

        return items;
    }
}
