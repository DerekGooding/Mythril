namespace Mythril.Data;

[Singleton]
public class StatAugments : ISubContent<Item, StatAugment[]>
{
    public StatAugment[] this[Item key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<Item, StatAugment[]> ByKey { get; } = [];

    public void Load(Dictionary<Item, StatAugment[]> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}
