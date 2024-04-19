using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestMonoGame.Data;

public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("R");
        writer.WriteValue(value.R);
        writer.WritePropertyName("G");
        writer.WriteValue(value.G);
        writer.WritePropertyName("B");
        writer.WriteValue(value.B);
        writer.WritePropertyName("A");
        writer.WriteValue(value.A);
        writer.WriteEndObject();
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var r = obj.GetValue("R")!.Value<byte>();
        var g = obj.GetValue("G")!.Value<byte>();
        var b = obj.GetValue("B")!.Value<byte>();
        var a = obj.GetValue("A")!.Value<byte>();
        return new Color(r, g, b, a);
    }
}