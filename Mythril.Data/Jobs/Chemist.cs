using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Chemist : Job
{
    [JsonConstructor]
    public Chemist(string name, string description, List<string> abilities)
        : base(name, description, abilities, JobType.Chemist)
    {
    }

    public Chemist() : base() { }
}
