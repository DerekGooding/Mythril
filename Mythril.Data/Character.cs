using Mythril.Data.Jobs;
using Newtonsoft.Json;

namespace Mythril.Data;

public class Character(string name, string jobName)
{
    public string Name { get; set; } = name;
    [JsonIgnore]
    public Job? Job { get; set; }
    public string JobName { get; set; } = jobName;
    public int JobPoints { get; set; } = 0;
    public int MaxHealth { get; set; } = 100;
    public int Health { get; set; } = 100;
    public int AttackPower { get; set; } = 10;
    public int Defense { get; set; } = 5;
}
