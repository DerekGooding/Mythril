using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Chemist : Job
{
    [JsonConstructor]
    public Chemist(string name, string description, List<string> abilities, int healthGrowth, int attackPowerGrowth, int defenseGrowth, List<int> jpLevels)
        : base(name, description, abilities, JobType.Chemist, healthGrowth, attackPowerGrowth, defenseGrowth, jpLevels)
    {
    }

    public Chemist() : base() { }
}
