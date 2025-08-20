using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.GameLogic.Items;

public class ItemConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Item);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var typeToken = jsonObject["Type"] ?? throw new JsonSerializationException("Item type is not defined.");
        var typeName = typeToken.Value<string>();
        Item item = typeName switch
        {
            "Consumable" => new ConsumableItem(),
            "Equipment" => new EquipmentItem(),
            // case "KeyItem":
            //     item = new KeyItem();
            //     break;
            _ => throw new JsonSerializationException($"Unknown item type: {typeName}"),
        };
        serializer.Populate(jsonObject.CreateReader(), item);
        return item;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
}
