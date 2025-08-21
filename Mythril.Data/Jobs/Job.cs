using Newtonsoft.Json;

namespace Mythril.Data.Jobs;

public enum JobType
{
    Squire,
    Chemist,
    Knight,
    Archer,
}

public abstract class Job
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Abilities { get; set; }
    public JobType Type { get; set; }
    public int HealthGrowth { get; set; }
    public int AttackPowerGrowth { get; set; }
    public int DefenseGrowth { get; set; }
    public List<int> JPLevels { get; set; }

    [JsonConstructor]
    protected Job(string name, string description, List<string> abilities, JobType type, int healthGrowth, int attackPowerGrowth, int defenseGrowth, List<int> jpLevels)
    {
        Name = name;
        Description = description;
        Abilities = abilities;
        Type = type;
        HealthGrowth = healthGrowth;
        AttackPowerGrowth = attackPowerGrowth;
        DefenseGrowth = defenseGrowth;
        JPLevels = jpLevels;
    }

    protected Job()
    {
        Name = string.Empty;
        Description = string.Empty;
        Abilities = [];
        JPLevels = [];
    }
}
