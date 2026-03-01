namespace Mythril.Data;

[Singleton]
public class QuestUnlocks : ISubContent<Quest, Quest[]>
{
    public Quest[] this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<Quest, Quest[]> ByKey { get; } = [];

    public void Load(Dictionary<Quest, Quest[]> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}
