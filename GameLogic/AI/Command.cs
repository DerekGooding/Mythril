using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mythril.GameLogic.AI
{
    public class Command
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("args")]
        public Dictionary<string, object> Args { get; set; }
    }
}
