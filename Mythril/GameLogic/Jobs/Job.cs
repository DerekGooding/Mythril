using Newtonsoft.Json;

namespace Mythril.GameLogic.Jobs;

public abstract class Job
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Abilities { get; set; }

    [JsonConstructor]
    protected Job(string name, string description, List<string> abilities)
    {
        Name = name;
        Description = description;
        Abilities = abilities;
    }

    protected Job()
    {
        Name = string.Empty;
        Description = string.Empty;
        Abilities = new List<string>();
    }
}
