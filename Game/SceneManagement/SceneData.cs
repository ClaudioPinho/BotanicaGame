using System.Collections.Generic;
using Newtonsoft.Json;

namespace TestMonoGame.Game.SceneManagement;

public struct SceneData
{
    [JsonProperty("name")] public string Name;
    [JsonProperty("skyboxTexture")] public string SkyboxTexture;
    [JsonProperty("sceneObjectsData")] public List<GameObjectData> SceneObjectsData;
}