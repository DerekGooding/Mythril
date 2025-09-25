namespace Mythril.Data;

[Singleton]
public class QuestUnlocks(Quests quests) : ISubContent<Quest, Quest[]>
{
    public Quest[] this[Quest key] => ByKey.TryGetValue(key, out var item) ? item : [];

    public Dictionary<Quest, Quest[]> ByKey { get; } = new()
    {
        { quests.TutorialSection, [ quests.Prologue ] },
        { quests.VisitStartingTown, [ quests.TutorialSection ] },
        { quests.BuyPotion, [ quests.VisitStartingTown ] },
        { quests.LearnAboutCadences, [ quests.VisitStartingTown ] },
        { quests.FarmGoblins, [ quests.VisitStartingTown ] },
        { quests.FarmTrents, [ quests.VisitStartingTown ] },
        { quests.FarmGolems, [ quests.VisitStartingTown ] },
        { quests.LearnabouttheMines, [ quests.LearnAboutCadences ] },
        { quests.FarmBats, [ quests.LearnabouttheMines ] },
        { quests.FarmSpiders, [ quests.LearnabouttheMines ] },
        { quests.FarmSlimes, [ quests.LearnabouttheMines ] },
        { quests.UnlockMining, [ quests.LearnabouttheMines ] },
        { quests.MineIronOre, [ quests.UnlockMining ] },
    };
}
