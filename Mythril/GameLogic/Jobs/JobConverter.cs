using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.GameLogic.Jobs;

public class JobConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Job);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        JToken? typeToken = jsonObject["Type"];
        if (typeToken == null)
        {
            throw new JsonSerializationException("Job type is not defined.");
        }
        string? typeName = typeToken.Value<string>();

        Job job;
        switch (typeName)
        {
            case "Squire":
                job = new Squire();
                break;
            case "Chemist":
                job = new Chemist();
                break;
            default:
                throw new JsonSerializationException($"Unknown job type: {typeName}");
        }

        serializer.Populate(jsonObject.CreateReader(), job);
        return job;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
}
