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

    [JsonConstructor]
    protected Job(string name, string description, List<string> abilities, JobType type)
    {
        Name = name;
        Description = description;
        Abilities = abilities;
        Type = type;
    }

    protected Job()
    {
        Name = string.Empty;
        Description = string.Empty;
        Abilities = [];
    }
}
