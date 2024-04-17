using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TestMonoGame.Game.Entities;

namespace TestMonoGame.Game;

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
            "Camera" => typeof(Camera),
            "Entity" => typeof(Entity),
            "Player" => typeof(Player),
            _ => null
        };
    }
}