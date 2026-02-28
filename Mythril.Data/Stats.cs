namespace Mythril.Data;

[Unique] public readonly partial record struct Stat;

[Singleton]
public partial class Stats : IContent<Stat>
{
       public Stat[] All { get; } =
    [
        new("Health", "The amount of damage a character can take before being defeated."),
        new("Strength", "Determines the physical power of a character."),
        new("Vitality", "Affects the speed and evasion of a character."),
        new("Magic", "Influences the effectiveness of magical abilities."),
        new("Spirit", ""),
        new("Speed", ""),
        new("Evade", ""),
        new("Hit", ""),
        new("Luck", ""),
    ];
}
