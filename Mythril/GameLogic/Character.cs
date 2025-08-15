using Mythril.GameLogic.Jobs;
using Newtonsoft.Json;

namespace Mythril.GameLogic;

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

    public void TakeDamage(int amount)
    {
        var damage = amount - Defense;
        if (damage < 0) damage = 0;

        Health -= damage;
        if (Health < 0) Health = 0;

        Game1.Log($"{Name} takes {damage} damage!");
    }
}
