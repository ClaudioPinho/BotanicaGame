using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotanicaGame.Data;

public class PointConverter : JsonConverter<Point>
{
    public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("X");
        writer.WriteValue(value.X);
        writer.WritePropertyName("Y");
        writer.WriteValue(value.Y);
        writer.WriteEndObject();
    }

    public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var x = obj.GetValue("X")!.Value<int>();
        var y = obj.GetValue("Y")!.Value<int>();
        return new Point(x, y);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
}