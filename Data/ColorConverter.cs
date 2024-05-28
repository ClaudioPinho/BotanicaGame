using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotanicaGame.Data;

public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue("#" + value.PackedValue.ToString("x8")[2..]);
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var hex = (string)reader.Value;

        if (string.IsNullOrEmpty(hex))
            return Color.CornflowerBlue;

        if (hex.StartsWith('#'))
            hex = hex[1..];

        switch (hex.Length)
        {
            // If no transparency specified, the color is opaque
            case 6:
                hex += "FF";
                break;
            // If transparency specified, ensure the hex is already in ARGB format
            case 8:
                break;
            default:
                throw new ArgumentException("Invalid hex color format.");
        }

        var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        var a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

        var color = new Color(r, g, b, a);

        return color;
    }
}