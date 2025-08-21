using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.Data.Jobs;

public class JobConverter : JsonConverter<Job>
{
    public override Job? ReadJson(JsonReader reader, Type objectType, Job? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject.Properties()
            .FirstOrDefault(p => string.Equals(p.Name, "Type", StringComparison.OrdinalIgnoreCase))
            ?.Value
            ?? throw new JsonSerializationException("Item type is not defined.");

        var typeString = typeToken.Value<string>() ?? throw new JsonSerializationException("Item type is null.");
        var typeEnum = Enum.TryParse<JobType>(typeString, true, out var typeInt)
            ? typeInt
            : throw new JsonSerializationException($"Unknown item type: {typeString}");

        Job job = typeEnum switch
        {
            JobType.Squire => new Squire(),
            JobType.Chemist => new Chemist(),
            JobType.Knight => new Knight(),
            JobType.Archer => new Archer(),
            _ => throw new JsonSerializationException($"Unknown job type: {typeEnum}"),
        };
        serializer.Populate(jsonObject.CreateReader(), job);
        return job;
    }

    public override void WriteJson(JsonWriter writer, Job? value, JsonSerializer serializer) => throw new NotImplementedException();
}
