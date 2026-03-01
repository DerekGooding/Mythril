namespace Mythril.Data;

[Singleton]
public class QuestToCadenceUnlocks : ISubContent<Quest, Cadence[]>
{
    public Cadence[] this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<Quest, Cadence[]> ByKey { get; } = [];

    public void Load(Dictionary<Quest, Cadence[]> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}
