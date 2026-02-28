namespace Mythril.Data;

[Unique] public readonly partial record struct Location;

[Singleton]
public partial class Locations(Quests quests) : IContent<Location>
{
    public Location[] All { get; } =
    [
        new("Village",
            [
                quests.Prologue,
                quests.TutorialSection,
                quests.VisitStartingTown,
                quests.BuyPotion,
                quests.LearnAboutCadences,
                quests.LearnabouttheMines,
            ]
        ),
        new("Dark Forest",
            [
                quests.FarmGoblins,
                quests.FarmTrents,
                quests.FarmGolems,
            ]
        ),
        new ("Iron Mines",
            [
                quests.FarmBats,
                quests.FarmSpiders,
                quests.FarmSlimes,
                quests.UnlockMining,
                quests.MineIronOre,
            ]
        ),
        new ("Ancient Ruins",
            [
                // Future quests can be added here
            ]
        ),
        new ("Dragon's Lair",
            [
                // Future quests can be added here
            ]
        ),

    ];
}
