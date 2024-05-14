using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BotanicaGame.Data;
using BotanicaGame.Debug;
using BotanicaGame.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;

namespace BotanicaGame.Game.SceneManagement;

public class SceneManager(ContentManager contentManager)
{
    public string SceneRoot = Directory.GetCurrentDirectory() + "/Content/Scenes";

    private readonly List<Scene> _loadedScenes = [];
    private readonly Queue<Scene> _scenesToAdd = new();
    private readonly Queue<Scene> _scenesToRemove = new();

#if DEVELOPMENT
    private const bool EnableHotReload = true;
    private readonly Dictionary<string, (FileSystemWatcher, Scene)> _loadedScenesWatchers = new();
#endif

    ~SceneManager()
    {
#if DEVELOPMENT
        foreach (var loadedScenesWatcher in _loadedScenesWatchers)
        {
            loadedScenesWatcher.Value.Item1.Dispose();
        }
#endif
    }

    public Scene Load(string sceneName, GamePhysics physicsContext = null)
    {
        var fullPath = GetFullScenePath(sceneName);
        if (!File.Exists(fullPath))
        {
            DebugUtils.PrintError($"Scene '{sceneName}' does not exist on path: '{fullPath}'");
        }
        else
        {
            try
            {
                var sceneData = RetrieveSceneDataFromPath(fullPath);
                if (sceneData == null)
                    return null;
                var scene = new Scene((SceneData)sceneData, contentManager, physicsContext);
                Load(scene);
#if DEVELOPMENT
                SubscribeToSceneFileChanges(sceneName, fullPath, scene);
#endif
                return scene;
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"An error occurred when trying to read the scene '{sceneName}'");
                DebugUtils.PrintException(e);
            }
        }
        return null;
    }

    public void Load(Scene scene)
    {
        if (_loadedScenes.Contains(scene))
        {
            DebugUtils.PrintError("Can't load a scene that has been already loaded by this scene manager!", this);
            return;
        }
        _scenesToAdd.Enqueue(scene);
        scene.Load();
    }

    public void Unload(Scene scene)
    {
        if (!_loadedScenes.Contains(scene))
        {
            DebugUtils.PrintError("Can't unload a scene that hasn't been loaded through this scene manager!", this);
            return;
        }
        scene.Unload();
        _scenesToRemove.Enqueue(scene);
#if DEVELOPMENT
        if (_loadedScenesWatchers.Any(x => x.Value.Item2 == scene))
        {
            var watcherToUnload = _loadedScenesWatchers.First(x => x.Value.Item2 == scene);
            watcherToUnload.Value.Item1.Dispose();
            _loadedScenesWatchers.Remove(watcherToUnload.Key);
        }
#endif
    }

    public void UpdateScenes(float deltaTime)
    {
        while (_scenesToRemove.Count != 0)
        {
            _loadedScenes.Remove(_scenesToRemove.Dequeue());
        }
        foreach (var loadedScene in _loadedScenes)
        {
            loadedScene.Update(deltaTime);
        }
        while (_scenesToAdd.Count != 0)
        {
            _loadedScenes.Add(_scenesToAdd.Dequeue());
        }
    }

    public void DrawScenes(GameTime gameTime)
    {
        foreach (var scene in _loadedScenes.Where(x => x.Visible))
        {
            scene.Draw(gameTime);
        }
    }

    private string GetFullScenePath(string sceneName)
    {
        return SceneRoot + $"/{sceneName}.json";
    }

    private static SceneData? RetrieveSceneDataFromPath(string path)
    {
        if (Path.Exists(path))
        {
            try
            {
                var sceneDataString = File.ReadAllText(path);
                if (string.IsNullOrEmpty(sceneDataString))
                    return null;
                return JsonConvert.DeserializeObject<SceneData>(sceneDataString, MainGame.JsonSerializerSettings);
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"An error occurred when retrieving scene data from path: '{path}'");
                DebugUtils.PrintException(e);
            }
        }
        else
        {
            DebugUtils.PrintError($"Invalid path provided for scene data retrieval: '{path}'");
        }

        return null;
    }

#if DEVELOPMENT
    private void SubscribeToSceneFileChanges(string sceneName, string path, Scene sceneLoaded)
    {
        if (string.IsNullOrEmpty(path) || sceneLoaded == null)
            return;
        var fileWatcher = new FileSystemWatcher();
        fileWatcher.Path = Path.GetDirectoryName(path);
        fileWatcher.Filter = Path.GetFileName(path);
        fileWatcher.Changed += (_, args) =>
        {
            if (args.ChangeType == WatcherChangeTypes.Changed)
            {
                OnSceneDataChanged(sceneName, sceneLoaded);
            }
        };
        fileWatcher.EnableRaisingEvents = true;
        _loadedScenesWatchers.Add(sceneName, (fileWatcher, sceneLoaded));
    }

    private void OnSceneDataChanged(string sceneName, Scene sceneLoaded)
    {
        if (!_loadedScenes.Contains(sceneLoaded)) return;
        // before trying to reload the scene we will check if the new scene data is valid
        var sceneData = RetrieveSceneDataFromPath(GetFullScenePath(sceneName));
        if (sceneData != null)
        {
            // proceed by unloading the previous scene
            // Unload(sceneLoaded);
            // and loading it again which will refresh it with the newest scene data
            // Load(sceneName);

            sceneLoaded.ReloadScene((SceneData)sceneData, contentManager);
            
            DebugUtils.PrintMessage($"Scene '{sceneName}' was reloaded!");
        }
        else
        {
            DebugUtils.PrintError($"Issue with reloading scene '{sceneName}'");
        }
    }
#endif
}