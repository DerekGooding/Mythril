namespace Mythril.Data;

[Singleton]
public class QuestToCadenceUnlocks(Quests quests, Cadences cadences) : ISubContent<Quest, Cadence[]>
{
    public Cadence[] this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<Quest, Cadence[]> ByKey { get; } = new()
    {
        { quests.LearnAboutCadences, [ cadences.Recruit, cadences.Apprentice, cadences.Student] },
    };
}
