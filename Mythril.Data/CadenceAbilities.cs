namespace Mythril.Data;

public readonly partial record struct CadenceAbility;

[Singleton]
public partial class CadenceAbilities : IContent<CadenceAbility>
{
    public CadenceAbility[] All { get; private set; } = [ new("Placeholder", "Dummy") ];

    public void Load(IEnumerable<CadenceAbility> data)
    {
        All = [.. data];
    }
}
