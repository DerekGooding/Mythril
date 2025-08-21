using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Squire : Job
{
    [JsonConstructor]
    public Squire(string name, string description, List<string> abilities, int healthGrowth, int attackPowerGrowth, int defenseGrowth, List<int> jpLevels)
        : base(name, description, abilities, JobType.Squire, healthGrowth, attackPowerGrowth, defenseGrowth, jpLevels)
    {
    }

    public Squire() : base() { }
}
