using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.GameLogic.Materia;

public class MateriaConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Materia);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject["Type"] ?? throw new JsonSerializationException("Materia type is not defined.");
        var typeName = typeToken.Value<string>();
        Materia materia = typeName switch
        {
            "Magic" => new MagicMateria(),
            "Summon" => new SummonMateria(),
            "Command" => new CommandMateria(),
            "Independent" => new IndependentMateria(),
            "Support" => new SupportMateria(),
            _ => throw new JsonSerializationException($"Unknown materia type: {typeName}"),
        };
        serializer.Populate(jsonObject.CreateReader(), materia);
        return materia;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
}
