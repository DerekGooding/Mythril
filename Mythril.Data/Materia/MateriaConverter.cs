using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.Data.Materia;

public class MateriaConverter : JsonConverter<Materia>
{
    public override Materia? ReadJson(JsonReader reader, Type objectType, Materia? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject.Properties()
            .FirstOrDefault(p => string.Equals(p.Name, "Type", StringComparison.OrdinalIgnoreCase))
            ?.Value
            ?? throw new JsonSerializationException("Item type is not defined.");

        var typeString = typeToken.Value<string>() ?? throw new JsonSerializationException("Item type is null.");
        var typeEnum = Enum.TryParse<MateriaType>(typeString, true, out var typeInt)
            ? typeInt
            : throw new JsonSerializationException($"Unknown item type: {typeString}");

        Materia materia = typeEnum switch
        {
            MateriaType.Magic => new MagicMateria(),
            MateriaType.Summon => new SummonMateria(),
            MateriaType.Command => new CommandMateria(),
            MateriaType.Independent => new IndependentMateria(),
            MateriaType.Support => new SupportMateria(),
            _ => throw new JsonSerializationException($"Unknown materia type: {typeEnum}"),
        };
        serializer.Populate(jsonObject.CreateReader(), materia);
        return materia;
    }

    public override void WriteJson(JsonWriter writer, Materia? value, JsonSerializer serializer) => throw new NotImplementedException();
}
