using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.GameLogic.Materia;

public class MateriaConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Materia);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        JToken? typeToken = jsonObject["Type"];
        if (typeToken == null)
        {
            throw new JsonSerializationException("Materia type is not defined.");
        }
        string? typeName = typeToken.Value<string>();

        Materia materia;
        switch (typeName)
        {
            case "Magic":
                materia = new MagicMateria();
                break;
            case "Summon":
                materia = new SummonMateria();
                break;
            case "Command":
                materia = new CommandMateria();
                break;
            case "Independent":
                materia = new IndependentMateria();
                break;
            case "Support":
                materia = new SupportMateria();
                break;
            default:
                throw new JsonSerializationException($"Unknown materia type: {typeName}");
        }

        serializer.Populate(jsonObject.CreateReader(), materia);
        return materia;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
}
