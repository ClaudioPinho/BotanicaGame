using System.Collections.Generic;
using Newtonsoft.Json;
using TestMonoGame.Game;

namespace TestMonoGame.Data;

public struct SceneData
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("skyboxTexture")] public string SkyboxTexture;
    [JsonProperty("sceneObjectsData")] public List<GameObjectData> SceneObjectsData;
}