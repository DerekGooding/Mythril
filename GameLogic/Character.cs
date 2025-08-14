using Mythril.GameLogic.Jobs;
using Newtonsoft.Json;

namespace Mythril.GameLogic
{
    public class Character
    {
        public string Name { get; set; }
        [JsonIgnore]
        public Job? Job { get; set; }
        public string JobName { get; set; }
        public int JobPoints { get; set; }

        public Character(string name, string jobName)
        {
            Name = name;
            JobName = jobName;
            JobPoints = 0;
        }
    }
}
