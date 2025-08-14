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
}
