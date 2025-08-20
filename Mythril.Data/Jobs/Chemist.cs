using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Chemist : Job
{
    [JsonConstructor]
    public Chemist(string name, string description, List<string> abilities)
        : base(name, description, abilities)
    {
    }

    public Chemist() : base() { }
}
