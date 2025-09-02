namespace Mythril.Data;


public readonly record struct StatAugment(Stat Stat, int ModifierAtFull);


[Singleton]
public class StatAugments(Stats stats, Items items) : ISubContent<Item, StatAugment[]>
{
    public StatAugment[] this[Item key] => ByKey[key];

    public Dictionary<Item, StatAugment[]> ByKey { get; } = new()
    {
        { items.FireI, [ new(stats.Strength, 20) ] },
    };
}
