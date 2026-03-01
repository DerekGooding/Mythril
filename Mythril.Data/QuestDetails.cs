namespace Mythril.Data;

[Singleton]
public class QuestDetails : ISubContent<Quest, QuestDetail>
{
    public QuestDetail this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : new(3, [], [], QuestType.Single);

    public Dictionary<Quest, QuestDetail> ByKey { get; } = [];

    public void Load(Dictionary<Quest, QuestDetail> data)
    {
        ByKey.Clear();
        foreach (var kvp in data) ByKey[kvp.Key] = kvp.Value;
    }
}
