using Mythril.Data.Items;

namespace Mythril.Data;

public class InventoryManager
{
    private readonly Dictionary<string, int> _resources = new();
    private readonly ResourceManager _resourceManager;

    public InventoryManager(ResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    public void Add(string name, int quantity = 1)
    {
        if (_resources.ContainsKey(name))
            _resources[name] += quantity;
        else
            _resources[name] = quantity;
    }

    public bool Remove(string name, int quantity = 1)
    {
        if (!_resources.ContainsKey(name) || _resources[name] < quantity)
            return false;

        _resources[name] -= quantity;
        if (_resources[name] == 0)
            _resources.Remove(name);

        return true;
    }

    public bool Has(string name, int quantity = 1)
    {
        return _resources.ContainsKey(name) && _resources[name] >= quantity;
    }

    public int GetQuantity(string name)
    {
        return _resources.GetValueOrDefault(name);
    }

    public IEnumerable<Item> GetItems()
    {
        var items = new List<Item>();
        foreach (var resource in _resources)
        {
            var item = _resourceManager.Items.FirstOrDefault(i => i.Name == resource.Key);
            if (item != null)
                items.Add(item);
        }

        return items;
    }
}
