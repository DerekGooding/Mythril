using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mythril.GameLogic.Items
{
    public class ItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Item);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            JToken? typeToken = jsonObject["Type"];
            if (typeToken == null)
            {
                throw new JsonSerializationException("Item type is not defined.");
            }
            string? typeName = typeToken.Value<string>();

            Item item;
            switch (typeName)
            {
                case "Consumable":
                    item = new ConsumableItem();
                    break;
                case "Equipment":
                    item = new EquipmentItem();
                    break;
                // case "KeyItem":
                //     item = new KeyItem();
                //     break;
                default:
                    throw new JsonSerializationException($"Unknown item type: {typeName}");
            }

            serializer.Populate(jsonObject.CreateReader(), item);
            return item;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
