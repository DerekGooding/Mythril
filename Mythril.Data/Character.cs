using Mythril.Data.Jobs;
using Newtonsoft.Json;

namespace Mythril.Data;

public class Character
{
    public string Name { get; set; }
    [JsonIgnore]
    public Job? Job { get; set; }
    public string JobName { get; set; }
    public int JobPoints { get; set; }
    public int MaxHealth { get; set; }
    public int Health { get; set; }
    public int AttackPower { get; set; }
    public int Defense { get; set; }

    public Character(string name, string jobName)
    {
        Name = name;
        JobName = jobName;
        JobPoints = 0;
        MaxHealth = 100;
        Health = 100;
        AttackPower = 10;
        Defense = 5;
    }
}
