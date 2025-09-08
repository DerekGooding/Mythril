namespace Mythril.Data;

[Singleton]
public class QuestUnlocks(Quests quests) : ISubContent<Quest, Quest[]>
{
    public Quest[] this[Quest key] => ByKey[key];

    public Dictionary<Quest, Quest[]> ByKey { get; } = new()
    {
        { quests.Prologue, [] },
        { quests.TutorialSection, [ quests.Prologue ] },
        { quests.VisitStartingTown, [ quests.TutorialSection ] },
        { quests.BuyPotion, [ quests.VisitStartingTown ] },
        { quests.LearnAboutCadences, [ quests.VisitStartingTown ] },
        { quests.FarmGoblins, [ quests.VisitStartingTown ] },
        { quests.FarmTrents, [ quests.VisitStartingTown ] },
        { quests.FarmGolems, [ quests.VisitStartingTown ] },
    };
}
