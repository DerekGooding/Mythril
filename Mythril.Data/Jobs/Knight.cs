using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Knight : Job
{
    [JsonConstructor]
    public Knight(string name, string description, List<string> abilities, int healthGrowth, int attackPowerGrowth, int defenseGrowth, List<int> jpLevels)
        : base(name, description, abilities, JobType.Knight, healthGrowth, attackPowerGrowth, defenseGrowth, jpLevels)
    {
    }

    public Knight() { }
}
