using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.Data.Items;

public class ItemConverter : JsonConverter<Item>
{
    public override Item? ReadJson(JsonReader reader, Type objectType, Item? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject.Properties()
            .FirstOrDefault(p => string.Equals(p.Name, "Type", StringComparison.OrdinalIgnoreCase))
            ?.Value
            ?? throw new JsonSerializationException("Item type is not defined.");

        var typeString = typeToken.Value<string>() ?? throw new JsonSerializationException("Item type is null.");
        var typeEnum = Enum.TryParse<ItemType>(typeString, true, out var typeInt)
            ? typeInt
            : throw new JsonSerializationException($"Unknown item type: {typeString}");

        Item item = typeInt switch
        {
            ItemType.Consumable => new ConsumableItem(),
            ItemType.Equipment => new EquipmentItem(),
            ItemType.Material => new MaterialItem(),
            _ => throw new JsonSerializationException($"Unknown item type: {typeInt}"),
        };
        serializer.Populate(jsonObject.CreateReader(), item);
        return item;
    }

    public override void WriteJson(JsonWriter writer, Item? value, JsonSerializer serializer) => throw new NotImplementedException();
}
