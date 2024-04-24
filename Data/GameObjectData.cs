using System;
using System.Collections.Generic;
using BotanicaGame.Game;
using BotanicaGame.Game.Entities;
using BotanicaGame.Game.UI;
using Newtonsoft.Json;

namespace BotanicaGame.Data;

public struct GameObjectData
{
    [JsonProperty("objectType")] public string ObjectType;

    [JsonProperty("name")] public string Name;

    [JsonProperty("params")] public Dictionary<string, object> Parameters;

    public static Type GetObjectTypeFromName(string typeName)
    {
        return typeName switch
        {
            "GameObject" => typeof(GameObject),
            "MeshObject" => typeof(MeshObject),
            "PhysicsObject" => typeof(PhysicsObject),
            "Canvas" => typeof(Canvas),
            "Camera" => typeof(Camera),
            "Entity" => typeof(Entity),
            "Player" => typeof(Player),
            _ => null
        };
    }
}