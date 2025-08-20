using Mythril.Data.Jobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Mythril.Data.Jobs;

public class JobConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Job);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject["Type"] ?? throw new JsonSerializationException("Job type is not defined.");
        var typeName = typeToken.Value<string>();
        Job job = typeName switch
        {
            "Squire" => new Squire(),
            "Chemist" => new Chemist(),
            "Knight" => new Knight(),
            "Archer" => new Archer(),
            _ => throw new JsonSerializationException($"Unknown job type: {typeName}"),
        };
        serializer.Populate(jsonObject.CreateReader(), job);
        return job;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
}
