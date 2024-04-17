using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestMonoGame.Debug;
using TestMonoGame.Physics;

namespace TestMonoGame.Game.SceneManagement;

public class Scene
{
    private readonly List<GameObject> _sceneObjects = [];
    private readonly List<IDrawable> _drawableObjects = [];

    private readonly Queue<GameObject> _gameObjectsToAdd = [];
    private readonly Queue<GameObject> _gameObjectsToRemove = [];

    private readonly Skybox _sceneSkybox;
    private readonly GamePhysics _physicsContext;

    public Scene(SceneData sceneData, ContentManager contentManager, GamePhysics physicsContext = null)
    {
        _physicsContext = physicsContext;
        if (!string.IsNullOrEmpty(sceneData.SkyboxTexture))
        {
            _sceneSkybox = new Skybox(sceneData.SkyboxTexture, contentManager);
        }

        // no data for this scene so we are going to simply ignore it
        if (sceneData.SceneObjectsData is not { Count: > 0 }) return;

        foreach (var gameObjectData in sceneData.SceneObjectsData)
        {
            var objectType = GameObjectData.GetObjectTypeFromName(gameObjectData.ObjectType);

            if (objectType == null)
            {
                DebugUtils.PrintError(
                    $"Couldn't not resolve string type '{gameObjectData.ObjectType}', ignoring...");
                continue;
            }

            try
            {
                var newGameObject = Activator.CreateInstance(objectType, args: gameObjectData.Name);

                foreach (var parameter in gameObjectData.Parameters)
                {
                    // todo: check if there's a better way to handle this
                    var member = objectType.GetMember(parameter.Key);
                    if (member.Length > 0)
                    {
                        if (member[0] is FieldInfo fieldInfo)
                        {
                            // check if the content manager can handle this type of data, if so we need to load from the content manager instead
                            if (fieldInfo.FieldType == typeof(Texture2D))
                            {
                                fieldInfo.SetValue(newGameObject,
                                    contentManager.Load<Texture2D>((string)parameter.Value));
                            }
                            else if (fieldInfo.FieldType == typeof(Model))
                            {
                                fieldInfo.SetValue(newGameObject,
                                    contentManager.Load<Model>((string)parameter.Value));
                            }
                            else if (fieldInfo.FieldType == typeof(Effect))
                            {
                                fieldInfo.SetValue(newGameObject,
                                    contentManager.Load<Effect>((string)parameter.Value));
                            }
                            else
                            {
                                fieldInfo.SetValue(newGameObject,
                                    parameter.Value is JObject jObject
                                        ? jObject.ToObject(fieldInfo.FieldType,
                                            JsonSerializer.CreateDefault(MainGame.JsonSerializerSettings))
                                        : parameter.Value);
                            }
                        }
                        else if (member[0] is PropertyInfo propertyInfo)
                        {
                            // check if the content manager can handle this type of data, if so we need to load from the content manager instead
                            if (propertyInfo.PropertyType == typeof(Texture2D))
                            {
                                propertyInfo.SetValue(newGameObject,
                                    contentManager.Load<Texture2D>((string)parameter.Value));
                            }
                            else if (propertyInfo.PropertyType == typeof(Model))
                            {
                                propertyInfo.SetValue(newGameObject,
                                    contentManager.Load<Model>((string)parameter.Value));
                            }
                            else if (propertyInfo.PropertyType == typeof(Effect))
                            {
                                propertyInfo.SetValue(newGameObject,
                                    contentManager.Load<Effect>((string)parameter.Value));
                            }
                            else
                            {
                                propertyInfo.SetValue(newGameObject,
                                    parameter.Value is JObject jObject
                                        ? jObject.ToObject(propertyInfo.PropertyType,
                                            JsonSerializer.CreateDefault(MainGame.JsonSerializerSettings))
                                        : parameter.Value);
                            }
                        }
                    }
                    else
                    {
                        DebugUtils.PrintWarning($"Member '{parameter.Key}' not found for " +
                                                $"object type '{objectType.Name}', ignoring member...");
                    }
                }

                AddNewGameObject(newGameObject as GameObject);
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"An issue occurred when trying to instantiate the game object of" +
                                      $" type '{gameObjectData.ObjectType}' with parameters: {JsonConvert.SerializeObject(gameObjectData.Parameters)}",
                    gameObjectData);
                DebugUtils.PrintException(e);
            }
        }
    }

    public void Initialize()
    {
        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Initialize();
        }
    }

    public void Update(float deltaTime)
    {
        // add any object that we marked for being added
        while (_gameObjectsToAdd.Count != 0)
        {
            AddGameObject(_gameObjectsToAdd.Dequeue());
        }

        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Update(deltaTime);
        }

        // remove the objects marked for clearing
        while (_gameObjectsToRemove.Count != 0)
        {
            RemoveGameObject(_gameObjectsToRemove.Dequeue());
        }
    }

    public void Draw(GameTime gameTime)
    {
        _sceneSkybox?.Draw();

        foreach (var drawableObject in _drawableObjects)
        {
            drawableObject.Draw(gameTime);
        }
    }

    public void AddNewGameObject(GameObject gameObject)
    {
        _gameObjectsToAdd.Enqueue(gameObject);
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        _gameObjectsToRemove.Enqueue(gameObject);
    }

    private void AddGameObject(GameObject gameObject)
    {
        if (_sceneObjects.Contains(gameObject))
        {
            DebugUtils.PrintMessage("Trying to add game object already registered!", gameObject);
            return;
        }

        // if (gameObject.SceneContext != this)
        // {
        //     DebugUtils.PrintError("Trying to add a GameObject that is not inside the same scene context", gameObject);
        //     return;
        // }

        _sceneObjects.Add(gameObject);

        if (gameObject is IDrawable drawableObject)
        {
            _drawableObjects.Add(drawableObject);
        }

        if (_physicsContext != null && gameObject is PhysicsObject physicsObject)
        {
            _physicsContext.AddPhysicsObject(physicsObject);
        }

        gameObject.SceneContext = this;
        gameObject.Initialize();
    }

    private void RemoveGameObject(GameObject gameObject)
    {
        if (!_sceneObjects.Contains(gameObject))
        {
            DebugUtils.PrintMessage("Trying to remove a game object that isn't registered!", gameObject);
            return;
        }

        // if (gameObject.SceneContext != this)
        // {
        //     DebugUtils.PrintError("Trying to remove a GameObject that is not inside the same scene context",
        //         gameObject);
        //     return;
        // }

        if (gameObject is IDrawable drawableObject)
        {
            _drawableObjects.Remove(drawableObject);
        }

        if (_physicsContext != null && gameObject is PhysicsObject physicsObject)
        {
            _physicsContext.RemovePhysicsObject(physicsObject);
        }

        _sceneObjects.Remove(gameObject);
        gameObject.Dispose();
    }
}