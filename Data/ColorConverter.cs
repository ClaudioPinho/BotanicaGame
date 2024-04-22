using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestMonoGame.Data;

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
            // if no transparency specified the color is opaque
            case 6:
                hex = "FF" + hex;
                break;
            // if transparency specified we need to flip the hex for R to A and vice-versa since we want the alpha to be last and XNA color has it at the start
            case 8:
            {
                var transparencyHex = hex.Substring(6, 2);
                hex = hex[..6];
                hex = transparencyHex + hex;
                break;
            }
        }

        var color = new Color(uint.Parse(hex, System.Globalization.NumberStyles.HexNumber));

        return color;
    }
}