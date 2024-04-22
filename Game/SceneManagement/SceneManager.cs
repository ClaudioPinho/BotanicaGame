using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;
using TestMonoGame.Data;
using TestMonoGame.Debug;
using TestMonoGame.Physics;

namespace TestMonoGame.Game.SceneManagement;

public class SceneManager(ContentManager contentManager)
{
    public string SceneRoot = Directory.GetCurrentDirectory() + "/Content/Scenes";

    private readonly List<Scene> _loadedScenes = [];

    public Scene Load(string sceneName, GamePhysics physicsContext = null)
    {
        var fullPath = SceneRoot + $"/{sceneName}.json";
        if (File.Exists(fullPath))
        {
            try
            {
                var sceneDataString = File.ReadAllText(fullPath);
                var sceneData = JsonConvert.DeserializeObject<SceneData>(sceneDataString, MainGame.JsonSerializerSettings);
                var scene = new Scene(sceneData, contentManager, physicsContext);
                Load(scene);
                return scene;
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"An error occurred when trying to read the scene '{sceneName}'");
                DebugUtils.PrintException(e);
            }
        }
        else
        {
            DebugUtils.PrintError($"Scene '{sceneName}' does not exist at: '{fullPath}'");
        }

        return null;
    }

    public void Load(Scene scene)
    {
        _loadedScenes.Add(scene);
        InitializeScene(scene);
    }

    public void Unload(Scene scene)
    {
        _loadedScenes.Remove(scene);
    }

    private void InitializeScene(Scene scene)
    {
        scene.Initialize();
    }

    public void UpdateScenes(float deltaTime)
    {
        foreach (var loadedScene in _loadedScenes)
        {
            loadedScene.Update(deltaTime);
        }
    }

    public void DrawScenes(GameTime gameTime)
    {
        foreach (var scene in _loadedScenes)
        {
            scene.Draw(gameTime);
        }
    }
}