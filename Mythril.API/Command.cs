using Newtonsoft.Json;

namespace Mythril.API;

public class Command
{
    [JsonProperty("action")]
    public string Action { get; set; } = string.Empty;

    [JsonProperty("target")]
    public string Target { get; set; } = string.Empty;

    [JsonProperty("args")]
    public Dictionary<string, object> Args { get; set; } = [];
}
