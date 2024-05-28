using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotanicaGame.Data;

public class Vector3NullConverter : JsonConverter<Vector3?>
{
    public override void WriteJson(JsonWriter writer, Vector3? value, JsonSerializer serializer)
    {
        var nonNullValues = value ?? Vector3.Zero;
        
        writer.WriteStartObject();
        writer.WritePropertyName("X");
        writer.WriteValue(nonNullValues.X);
        writer.WritePropertyName("Y");
        writer.WriteValue(nonNullValues.Y);
        writer.WritePropertyName("Z");
        writer.WriteValue(nonNullValues.Z);
        writer.WriteEndObject();
    }

    public override Vector3? ReadJson(JsonReader reader, Type objectType, Vector3? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        var x = obj.GetValue("X")!.Value<float>();
        var y = obj.GetValue("Y")!.Value<float>();
        var z = obj.GetValue("Z")!.Value<float>();
        return new Vector3(x, y, z);
    }
    
    public override bool CanRead => true;
    public override bool CanWrite => true;
}