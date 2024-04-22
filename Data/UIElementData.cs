using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TestMonoGame.Game.UI;

namespace TestMonoGame.Data;

public class UIElementData
{
    [JsonProperty("objectType")] public string ObjectType;
    
    [JsonProperty("name")] public string Name;

    [JsonProperty("params")] public Dictionary<string, object> Parameters;

    public static Type GetObjectTypeFromName(string typeName)
    {
        return typeName switch
        {
            "Image" => typeof(UIImage),
            "Text" => typeof(UIText),
            // "Button" => typeof(UIGraphics),
            _ => null
        };
    }
}