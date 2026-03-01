namespace Mythril.Data;

[Singleton]
public class AbilityAugments : ISubContent<CadenceAbility, Stat>
{
    public Stat this[CadenceAbility key] => ByKey.TryGetValue(key, out var item) ? item : new("None", "");
    public Dictionary<CadenceAbility, Stat> ByKey { get; } = [];

    public void Load(Dictionary<CadenceAbility, Stat> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}
