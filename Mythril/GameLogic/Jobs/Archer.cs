using Newtonsoft.Json;

namespace Mythril.GameLogic.Jobs;

public class Archer : Job
{
    [JsonConstructor]
    public Archer(string name, string description, List<string> abilities)
        : base(name, description, abilities)
    {
    }

    public Archer() : base() { }
}
