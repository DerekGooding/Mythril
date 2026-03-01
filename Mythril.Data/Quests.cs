namespace Mythril.Data;

public partial record struct Quest;

[Singleton]
public partial class Quests : IContent<Quest>
{
    public Quest[] All { get; private set; } = [ new("Placeholder", "Dummy") ];

    public void Load(IEnumerable<Quest> data)
    {
        All = [.. data];
    }
}
