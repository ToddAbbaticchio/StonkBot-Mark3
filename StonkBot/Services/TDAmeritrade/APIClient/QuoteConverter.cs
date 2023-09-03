using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StonkBot.Services.TDAmeritrade.APIClient.Models;

namespace StonkBot.Services.TDAmeritrade.APIClient;

public class QuoteConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var list = (QuoteList)value!;

        writer.WriteStartObject();

        foreach (var p in list.Info!)
        {
            writer.WritePropertyName(p.symbol!);
            serializer.Serialize(writer, p);
        }

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jo = serializer.Deserialize<JObject>(reader);
        var result = new QuoteList { Info = new List<Quote>() };

        foreach (var prop in jo!.Properties())
        {
            var p = prop.Value.ToObject<Quote>();
            // set name from property name
            p!.symbol = prop.Name;
            result.Info.Add(p);
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(QuoteList);
    }
}