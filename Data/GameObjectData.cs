using System;
using System.Collections.Generic;
using BotanicaGame.Game;
using BotanicaGame.Game.Entities;
using BotanicaGame.Game.UI;
using Newtonsoft.Json;

namespace BotanicaGame.Data;

public struct GameObjectData
{
    [JsonProperty("id")] public string Id;

    [JsonProperty("objectType")] public string ObjectType;

    [JsonProperty("name")] public string Name;

    [JsonProperty("parentObject")] public string ParentObject;
    
    [JsonProperty("scripts")] public List<string> Scripts;
    
    [JsonProperty("params")] public Dictionary<string, object> Parameters;

    public static Type GetObjectTypeFromName(string typeName)
    {
        return typeName switch
        {
            "GameObject" => typeof(GameObject),
            "MeshObject" => typeof(MeshObject),
            "PhysicsObject" => typeof(PhysicsObject),
            "Graphic" => typeof(UIGraphic),
            "Canvas" => typeof(Canvas),
            "Camera" => typeof(Camera),
            "Entity" => typeof(Entity),
            "Image" => typeof(UIImage),
            "Text" => typeof(UIText),
            "Button" => typeof(UIButton),
            _ => null
        };
    }
}