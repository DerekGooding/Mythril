namespace Mythril.Data;

[Singleton]
public class StatAugments(Stats stats, Items items) : ISubContent<Item, StatAugment[]>
{
    public StatAugment[] this[Item key] => ByKey[key];

    public Dictionary<Item, StatAugment[]> ByKey { get; } = new()
    {
        { items.FireI, [ new(stats.Strength, 20) ] },
        { items.IceI,  [ new(stats.Magic, 20) ]  },
    };
}
