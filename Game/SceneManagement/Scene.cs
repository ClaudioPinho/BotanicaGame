using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BotanicaGame.Data;
using BotanicaGame.Debug;
using BotanicaGame.Game.UI;
using BotanicaGame.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BotanicaGame.Game.SceneManagement;

public class Scene : IDrawable
{
    public enum ESceneState
    {
        Unloaded = 0,
        Loaded = 1
    }
    
    public string Name { get; private set; }

    public int DrawOrder { get; }
    public bool Visible { get; set; } = true;
    public event EventHandler<EventArgs> DrawOrderChanged;

    public event EventHandler<EventArgs> VisibleChanged;
    
    public event Action OnSceneUnload;
    public event Action OnSceneLoad;
    public event Action OnSceneReloaded;

    private readonly List<GameObject> _sceneObjects = [];
    private readonly List<IDrawable> _drawableObjects = [];

    private readonly Queue<GameObject> _gameObjectsToAdd = [];
    private readonly Queue<GameObject> _gameObjectsToRemove = [];

    private readonly Skybox _sceneSkybox;
    private readonly GamePhysics _physicsContext;

    private ESceneState _currentSceneState = ESceneState.Unloaded;

    public Scene(SceneData sceneData, ContentManager contentManager, GamePhysics physicsContext = null)
    {
        Name = sceneData.Name;
        
        _physicsContext = physicsContext;
        if (!string.IsNullOrEmpty(sceneData.SkyboxTexture))
        {
            _sceneSkybox = new Skybox(sceneData.SkyboxTexture, contentManager);
        }

        // no data for this scene so we are going to simply ignore it
        if (sceneData.SceneObjectsData is not { Count: > 0 }) return;

        LoadSceneData(sceneData, contentManager);
    }

    public void Update(float deltaTime)
    {
        // we shouldn't update this scene if it's unloaded!
        if (_currentSceneState == ESceneState.Unloaded) return;

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

        foreach (var drawableObject in _drawableObjects.Where(x => x.Visible).OrderBy(x => x.DrawOrder))
        {
            drawableObject.Draw(gameTime);
        }
    }

    public void Load()
    {
        foreach (var sceneObject in _sceneObjects)
        {
            sceneObject.Initialize();
        }
        _currentSceneState = ESceneState.Loaded;
        try
        {
            OnSceneLoad?.Invoke();
        }
        catch (Exception e)
        {
            DebugUtils.PrintException(e, this);
        }
    }

    public void Unload()
    {
        _sceneObjects.ForEach(DestroyGameObject);
        _currentSceneState = ESceneState.Unloaded;
        try
        {
            OnSceneUnload?.Invoke();
        }
        catch (Exception e)
        {
            DebugUtils.PrintException(e, this);
        }
    }

    public void ReloadScene(SceneData sceneData, ContentManager contentManager)
    {
        // no data for this scene so we are going to simply ignore it
        if (sceneData.SceneObjectsData is not { Count: > 0 }) return;

        // Unload();
        // LoadSceneData(sceneData, contentManager);

        // reloads the objects already in the scene
        foreach (var sceneObject in sceneData.SceneObjectsData)
        {
            var correspondentObject = GetGameObjectById(sceneObject.Id);
            if (correspondentObject == null) continue;
            LoadObjectParameters(correspondentObject.GetType(), correspondentObject, sceneObject.Parameters, contentManager);
        }

        try
        {
            OnSceneReloaded?.Invoke();
        }
        catch (Exception e)
        {
            DebugUtils.PrintException(e);
        }
    }

    public T GetGameObjectOfType<T>() where T : GameObject
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.First(x => x is T) as T;
    }

    public IEnumerable<T> GetGameObjectsOfType<T>() where T : GameObject
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.OfType<T>();
    }

    public T GetGameObjectByName<T>(string name) where T : GameObject
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.FirstOrDefault(x => x.Name == name && x is T) as T;
    }
    
    public GameObject GetGameObjectByName(string name)
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.FirstOrDefault(x => x.Name == name);
    }

    public T GetGameObjectById<T>(string id) where T : GameObject
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.FirstOrDefault(x => x.Id == id) as T;
    }

    public GameObject GetGameObjectById(string id)
    {
        if (_sceneObjects == null || _sceneObjects.Count == 0)
            return null;
        return _sceneObjects.FirstOrDefault(x => x.Id == id);
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

        _sceneObjects.Add(gameObject);

        // todo: temporary solution but this is bad
        if (gameObject is IDrawable drawableObject and not UIGraphic)
            _drawableObjects.Add(drawableObject);

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

        // todo: temporary solution but this is bad
        if (gameObject is IDrawable drawableObject and not UIGraphic)
            _drawableObjects.Remove(drawableObject);

        if (_physicsContext != null && gameObject is PhysicsObject physicsObject)
        {
            _physicsContext.RemovePhysicsObject(physicsObject);
        }

        _sceneObjects.Remove(gameObject);
        gameObject.Dispose();
    }

    // todo: remove scene loading logic from the Scene class, it makes no sense to exist here
    private void LoadSceneData(SceneData sceneData, ContentManager contentManager)
    {
        var loadedObjects = new Dictionary<GameObject, GameObjectData>();

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
                // check if this object was created, if not just ignore it and move to the next
                if (Activator.CreateInstance(objectType, args: gameObjectData.Id) is not GameObject newGameObject)
                {
                    DebugUtils.PrintError($"Couldn't create object with id {gameObjectData.Id}, ignoring...");
                    continue;
                }

                newGameObject.Name = gameObjectData.Name;

                // add the created object for later processing
                loadedObjects.Add(newGameObject, gameObjectData);
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"An issue occurred when trying to instantiate the game object of" +
                                      $" type '{objectType.Name}' with parameters: {JsonConvert.SerializeObject(gameObjectData.Parameters)}",
                    gameObjectData);
                DebugUtils.PrintException(e);
            }
        }

        // now that the objects have been loaded we need to load its data
        foreach (var (loadedObject, loadedObjectData) in loadedObjects)
        {
            try
            {
                // set the proper parent if it exists
                if (!string.IsNullOrEmpty(loadedObjectData.ParentObject))
                {
                    var parentObject = loadedObjects
                        .FirstOrDefault(x => x.Key.Id == loadedObjectData.ParentObject).Key;
                    if (parentObject == null)
                    {
                        DebugUtils.PrintError($"Couldn't find parent with id: " +
                                              $"{loadedObjectData.ParentObject}", loadedObject);
                    }
                    else
                    {
                        loadedObject.Parent = parentObject;
                    }
                }

                LoadObjectParameters(loadedObject.GetType(), loadedObject, loadedObjectData.Parameters,
                    contentManager);
                
                if (loadedObjectData.Scripts != null && loadedObjectData.Scripts.Count != 0)
                {
                    foreach (var scriptName in loadedObjectData.Scripts.Where(scriptName => !string.IsNullOrEmpty(scriptName)))
                    {
                        try
                        {
                            var scriptType = Type.GetType(scriptName);
                            if (scriptType == null)
                            {
                                DebugUtils.PrintError($"Script named '{scriptName}' couldn't be found, ignoring...", this);
                                continue;
                            }
                            var externalScript = Activator.CreateInstance(scriptType) as IExternalScript;
                            loadedObject.AddExternalScript(externalScript);
                        }
                        catch (Exception e)
                        {
                            DebugUtils.PrintError($"An error occurred when trying to add a script named '{scriptName}'", this);
                            DebugUtils.PrintException(e);
                        }
                    }
                }

                AddGameObject(loadedObject);
            }
            catch (Exception e)
            {
                DebugUtils.PrintError(
                    $"An issue occurred when trying to load data for the object id '{loadedObjectData.Id}'",
                    loadedObjectData);
                DebugUtils.PrintException(e);
            }
        }
    }

    private void LoadObjectParameters(Type objectType, GameObject gameObject,
        Dictionary<string, object> objectParameters, ContentManager contentManager)
    {
        foreach (var parameter in objectParameters)
        {
            // todo: check if there's a better way to handle this
            var member = objectType.GetMember(parameter.Key);
            if (member.Length <= 0) continue;
            switch (member[0])
            {
                // check if the content manager can handle this type of data, if so we need to load from the content manager instead

                case FieldInfo fieldInfo when fieldInfo.FieldType.IsAssignableTo(typeof(GameObject)):
                    fieldInfo.SetValue(gameObject, GetGameObjectById((string)parameter.Value));
                    break;

                // case FieldInfo fieldInfo when fieldInfo.FieldType.IsAssignableTo(typeof(IEnumerable<GameObject>)):
                //     
                //     fieldInfo.SetValue(createdObject, GetGameObjectById((string)parameter.Value));
                //     break;

                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Texture2D):
                    fieldInfo.SetValue(gameObject,
                        contentManager.Load<Texture2D>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(SoundEffect):
                    fieldInfo.SetValue(gameObject,
                        contentManager.Load<SoundEffect>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(SpriteFont):
                    fieldInfo.SetValue(gameObject,
                        contentManager.Load<SpriteFont>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Model):
                    fieldInfo.SetValue(gameObject,
                        contentManager.Load<Model>((string)parameter.Value));
                    break;
                case FieldInfo fieldInfo when fieldInfo.FieldType == typeof(Effect):
                    fieldInfo.SetValue(gameObject,
                        contentManager.Load<Effect>((string)parameter.Value));
                    break;

                case FieldInfo fieldInfo:
                {
                    // if type matches no further processing required
                    if (parameter.Value.GetType() == fieldInfo.FieldType)
                    {
                        fieldInfo.SetValue(gameObject, parameter.Value);
                    }
                    else if (TypeDescriptor.GetConverter(parameter.Value).CanConvertTo(fieldInfo.FieldType))
                    {
                        fieldInfo.SetValue(gameObject,
                            TypeDescriptor.GetConverter(parameter.Value)
                                .ConvertTo(parameter.Value, fieldInfo.FieldType));
                    }
                    else
                        switch (parameter.Value)
                        {
                            // if the type came as a 'JObject' we will try to retrieve it as the correct type
                            case JObject jObject:
                                fieldInfo.SetValue(gameObject,
                                    jObject.ToObject(fieldInfo.FieldType, MainGame.JsonSerializer));
                                break;
                            // if it's a string then it's highly likely that is some sort of json string representing the object itself
                            case string stringData:
                                fieldInfo.SetValue(gameObject,
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

                case PropertyInfo propertyInfo when propertyInfo.PropertyType.IsAssignableTo(typeof(GameObject)):
                    propertyInfo.SetValue(gameObject, GetGameObjectById((string)parameter.Value));
                    break;

                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Texture2D):
                    propertyInfo.SetValue(gameObject,
                        contentManager.Load<Texture2D>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(SoundEffect):
                    propertyInfo.SetValue(gameObject,
                        contentManager.Load<SoundEffect>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(SpriteFont):
                    propertyInfo.SetValue(gameObject,
                        contentManager.Load<SpriteFont>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Model):
                    propertyInfo.SetValue(gameObject,
                        contentManager.Load<Model>((string)parameter.Value));
                    break;
                case PropertyInfo propertyInfo when propertyInfo.PropertyType == typeof(Effect):
                    propertyInfo.SetValue(gameObject,
                        contentManager.Load<Effect>((string)parameter.Value));
                    break;

                case PropertyInfo propertyInfo:
                {
                    // if type matches no further processing required
                    if (parameter.Value.GetType() == propertyInfo.PropertyType)
                    {
                        propertyInfo.SetValue(gameObject, parameter.Value);
                    }
                    else if (TypeDescriptor.GetConverter(parameter.Value).CanConvertTo(propertyInfo.PropertyType))
                    {
                        propertyInfo.SetValue(gameObject,
                            TypeDescriptor.GetConverter(parameter.Value)
                                .ConvertTo(parameter.Value, propertyInfo.PropertyType));
                    }
                    else
                        switch (parameter.Value)
                        {
                            // if the type came as a 'JObject' we will try to retrieve it as the correct type
                            case JObject jObject:
                                propertyInfo.SetValue(gameObject,
                                    jObject.ToObject(propertyInfo.PropertyType, MainGame.JsonSerializer));
                                break;
                            // if it's a string then it's highly likely that is some sort of json string representing the object itself
                            case string stringData:
                                propertyInfo.SetValue(gameObject,
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