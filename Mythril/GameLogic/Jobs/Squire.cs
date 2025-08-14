using Newtonsoft.Json;

namespace Mythril.GameLogic.Jobs;

public class Squire : Job
{
    [JsonConstructor]
    public Squire(string name, string description, List<string> abilities)
        : base(name, description, abilities)
    {
    }

    public Squire() : base() { }
}
