using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotanicaGame.Data;

public class Vector2NullConverter : JsonConverter<Vector2?>
{
    public override void WriteJson(JsonWriter writer, Vector2? value, JsonSerializer serializer)
    {
        var nonNullValues = value ?? Vector2.Zero;
        
        writer.WriteStartObject();
        writer.WritePropertyName("X");
        writer.WriteValue(nonNullValues.X);
        writer.WritePropertyName("Y");
        writer.WriteValue(nonNullValues.Y);
        writer.WriteEndObject();
    }

    public override Vector2? ReadJson(JsonReader reader, Type objectType, Vector2? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var x = obj.GetValue("X")!.Value<float>();
        var y = obj.GetValue("Y")!.Value<float>();
        return new Vector2(x, y);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}