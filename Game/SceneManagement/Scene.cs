using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestMonoGame.Data;
using TestMonoGame.Debug;
using TestMonoGame.Game.UI;
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
                // create the new game object
                var newGameObject = Activator.CreateInstance(objectType, args: gameObjectData.Name) as GameObject;

                if (newGameObject is Canvas canvasObject)
                {
                    if (!gameObjectData.Parameters.TryGetValue("UIElements", out var uiElementsRaw))
                    {
                        DebugUtils.PrintWarning("Canvas object detected but no UIElements present, ignoring...");
                        return;
                    }

                    var uiElements = ((JArray)uiElementsRaw).ToObject<List<UIElementData>>(MainGame.JsonSerializer);

                    foreach (var uiElement in uiElements)
                    {
                        var uiElementType = UIElementData.GetObjectTypeFromName(uiElement.ObjectType);
                        var newUIElement = Activator.CreateInstance(uiElementType, args: canvasObject) as UIGraphics;

                        newUIElement.Name = uiElement.Name;

                        // load this UI graphic parameters
                        LoadObjectParameters(uiElementType, newUIElement, uiElement.Parameters, contentManager);

                        // add this UI graphic to the canvas
                        canvasObject.AddUIGraphic(newUIElement);
                    }
                }

                // Load this object parameters
                LoadObjectParameters(objectType, newGameObject, gameObjectData.Parameters, contentManager);

                AddGameObject(newGameObject);
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

    public T GetGameObjectOfType<T>() where T : GameObject
    {
        return _sceneObjects.First(x => x is T) as T;
    }

    public IEnumerable<T> GetGameObjectsOfType<T>() where T : GameObject
    {
        return _sceneObjects.OfType<T>();
    }

    public T GetGameObjectOfName<T>(string name) where T : GameObject
    {
        return _sceneObjects.First(x => x.Name == name && x is T) as T;
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

    private void LoadObjectParameters(Type objectType, object createdObject,
        Dictionary<string, object> objectParameters, ContentManager contentManager)
    {
        foreach (var parameter in objectParameters)
        {
            // ignore the UIElements section from the main canvas
            if (createdObject is Canvas && parameter.Key == "UIElements") continue;

            // todo: check if there's a better way to handle this
            var member = objectType.GetMember(parameter.Key);
            if (member.Length <= 0) continue;
            switch (member[0])
            {
                // check if the content manager can handle this type of data, if so we need to load from the content manager instead
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Texture2D):
                    fieldInfo.SetValue(createdObject,
                        contentManager.Load<Texture2D>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(SpriteFont):
                    fieldInfo.SetValue(createdObject,
                        contentManager.Load<SpriteFont>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Model):
                    fieldInfo.SetValue(createdObject,
                        contentManager.Load<Model>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Effect):
                    fieldInfo.SetValue(createdObject,
                        contentManager.Load<Effect>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo:
                {
                    // if type matches no further processing required
                    if (parameter.Value.GetType() == fieldInfo.FieldType)
                    {
                        fieldInfo.SetValue(createdObject, parameter.Value);
                    }
                    else if (TypeDescriptor.GetConverter(parameter.Value).CanConvertTo(fieldInfo.FieldType))
                    {
                        fieldInfo.SetValue(createdObject,
                            TypeDescriptor.GetConverter(parameter.Value)
                                .ConvertTo(parameter.Value, fieldInfo.FieldType));
                    }
                    else
                        switch (parameter.Value)
                        {
                            // if the type came as a 'JObject' we will try to retrieve it as the correct type
                            case JObject jObject:
                                fieldInfo.SetValue(createdObject,
                                    jObject.ToObject(fieldInfo.FieldType, MainGame.JsonSerializer));
                                break;
                            // if it's a string then it's highly likely that is some sort of json string representing the object itself
                            case string stringData:
                                fieldInfo.SetValue(createdObject,
                                    JsonConvert.DeserializeObject($"\"{stringData}\"", fieldInfo.FieldType,
                                        MainGame.JsonSerializerSettings));
                                break;
                            default:
                                DebugUtils.PrintWarning($"Unknown data being set object of type " +
                                                        $"'{fieldInfo.FieldType}' with value: '{parameter.Value}', ignoring...");
                                break;
                        }

                    break;
                }
                // check if the content manager can handle this type of data, if so we need to load from the content manager instead
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Texture2D):
                    propertyInfo.SetValue(createdObject,
                        contentManager.Load<Texture2D>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(SpriteFont):
                    propertyInfo.SetValue(createdObject,
                        contentManager.Load<SpriteFont>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Model):
                    propertyInfo.SetValue(createdObject,
                        contentManager.Load<Model>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Effect):
                    propertyInfo.SetValue(createdObject,
                        contentManager.Load<Effect>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo:
                {
                    // if type matches no further processing required
                    if (parameter.Value.GetType() == propertyInfo.PropertyType)
                    {
                        propertyInfo.SetValue(createdObject, parameter.Value);
                    }
                    else if (TypeDescriptor.GetConverter(parameter.Value).CanConvertTo(propertyInfo.PropertyType))
                    {
                        propertyInfo.SetValue(createdObject,
                            TypeDescriptor.GetConverter(parameter.Value)
                                .ConvertTo(parameter.Value, propertyInfo.PropertyType));
                    }
                    else
                        switch (parameter.Value)
                        {
                            // if the type came as a 'JObject' we will try to retrieve it as the correct type
                            case JObject jObject:
                                propertyInfo.SetValue(createdObject,
                                    jObject.ToObject(propertyInfo.PropertyType, MainGame.JsonSerializer));
                                break;
                            // if it's a string then it's highly likely that is some sort of json string representing the object itself
                            case string stringData:
                                propertyInfo.SetValue(createdObject,
                                    JsonConvert.DeserializeObject($"\"{stringData}\"", propertyInfo.PropertyType,
                                        MainGame.JsonSerializerSettings));
                                break;
                            default:
                                DebugUtils.PrintWarning($"Unknown data being set object of type " +
                                                        $"'{propertyInfo.PropertyType}' with value: '{parameter.Value}', ignoring...");
                                break;
                        }

                    break;
                }
            }
        }
    }
}