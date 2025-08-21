using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Squire : Job
{
    [JsonConstructor]
    public Squire(string name, string description, List<string> abilities)
        : base(name, description, abilities, JobType.Squire)
    {
    }

    public Squire() : base() { }
}
