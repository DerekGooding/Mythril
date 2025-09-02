namespace Mythril.Data;

[Singleton]
public class AbilityAugments(CadenceAbilities abilities, Stats stats) : ISubContent<CadenceAbility, Stat>
{
    public Stat this[CadenceAbility key] => ByKey[key];
    public Dictionary<CadenceAbility, Stat> ByKey { get; } = new()
    {
        { abilities.AugmentStrength, stats.Strength },
    };
}