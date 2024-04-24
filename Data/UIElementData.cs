using System;
using System.Collections.Generic;
using BotanicaGame.Game.UI;
using Newtonsoft.Json;

namespace BotanicaGame.Data;

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
            "Button" => typeof(UIButton),
            _ => null
        };
    }
}