namespace Mythril.Data;

[Singleton]
public class AbilityAugments(CadenceAbilities abilities, Stats stats) : ISubContent<CadenceAbility, Stat>
{
    public Stat this[CadenceAbility key] => ByKey[key];
    public Dictionary<CadenceAbility, Stat> ByKey { get; } = new()
    {
        { abilities.AugmentStrength, stats.Strength },
        { abilities.AugmentHealth, stats.Health },
        { abilities.AugmentVitality, stats.Vitality },
        { abilities.AugmentSpirit, stats.Spirit },
        { abilities.AugmentMagic, stats.Magic },
        { abilities.AugmentHit, stats.Hit },
        { abilities.AugmentEvade, stats.Evade },
        { abilities.AugmentSpeed, stats.Speed },
        { abilities.AugmentLuck, stats.Luck },
    };
}