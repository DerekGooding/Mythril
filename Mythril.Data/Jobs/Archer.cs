using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public class Archer : Job
{
    [JsonConstructor]
    public Archer(string name, string description, List<string> abilities, int healthGrowth, int attackPowerGrowth, int defenseGrowth, List<int> jpLevels)
        : base(name, description, abilities, JobType.Archer, healthGrowth, attackPowerGrowth, defenseGrowth, jpLevels)
    {
    }

    public Archer() : base() { }
}
