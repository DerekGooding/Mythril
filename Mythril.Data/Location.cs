namespace Mythril.Data;

public readonly record struct Location(string Name, Quest[] Quests) : INamed;


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
                quests.UnlockStrengthJunction,
                quests.UnlockFireRefineAbility
            ]
        ),
        new("Dark Forest",
            [
                quests.FarmGoblins,
            ]
        )
    ];
}