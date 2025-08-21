using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Knight : Job
{
    [JsonConstructor]
    public Knight(string name, string description, List<string> abilities)
        : base(name, description, abilities, JobType.Knight)
    {
    }

    public Knight() : base() { }
}
