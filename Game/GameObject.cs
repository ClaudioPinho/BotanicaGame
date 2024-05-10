using System;
using System.Collections.Generic;
using System.Linq;
using BotanicaGame.Debug;
using BotanicaGame.Game.SceneManagement;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Game;

public class GameObject(string id = null) : IDisposable
{
    public bool IsActive = true;

    public readonly string Id = id ?? Guid.NewGuid().ToString();

    public string Name = "GameObject";

    public bool CanOcclude = true;

    public readonly Transform Transform = new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public GameObject Parent { get; private set; }

    public Scene SceneContext { get; set; }

    private readonly List<IExternalScript> _externalScripts = [];
    private readonly Queue<IExternalScript> _scriptsToAdd = new Queue<IExternalScript>();
    private readonly Queue<IExternalScript> _scriptsToRemove = new Queue<IExternalScript>();

    private bool _isInitialized;

    public void AddExternalScript(IExternalScript externalScript) => _scriptsToAdd.Enqueue(externalScript);

    public void RemoveExternalScript(IExternalScript externalScript) => _scriptsToRemove.Enqueue(externalScript);

    public T FindInParents<T>()
    {
        switch (Parent)
        {
            case null:
                return default;
            case T found:
                return found;
            default:
            {
                if (Parent.Parent != null)
                {
                    return Parent.Parent.FindInParents<T>();
                }
                break;
            }
        }
        return default;
    }

    public T FindInParent<T>()
    {
        return Parent switch
        {
            null => default,
            T found => found,
            _ => default
        };
    }

    public virtual void Initialize()
    {
        foreach (var externalScript in _externalScripts.Where(x => !x.IsInitialized()))
        {
            try
            {
                externalScript.Start(this);
            }
            catch (Exception e)
            {
                DebugUtils.PrintException(e);
            }
        }
        _isInitialized = true;
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var externalScript in _externalScripts)
        {
            try
            {
                externalScript.Update(deltaTime);
            }
            catch (Exception e)
            {
                DebugUtils.PrintException(e);
            }
        }
        HandleExternalScriptChanges();
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void SetParent(GameObject parentObject)
    {
        Parent = parentObject;
    }

    protected virtual void OnExternalScriptAdded(IExternalScript externalScript)
    {
        if (_externalScripts.Contains(externalScript)) return;
        _externalScripts.Add(externalScript);
        if (_isInitialized)
        {
            try
            {
                externalScript.Start(this);
            }
            catch (Exception e)
            {
                DebugUtils.PrintException(e);
            }
        }
    }

    protected virtual void OnExternalScriptRemoved(IExternalScript externalScript)
    {
        if (!_externalScripts.Contains(externalScript)) return;
        _externalScripts.Remove(externalScript);
    }

    private void HandleExternalScriptChanges()
    {
        while (_scriptsToAdd.Count > 0)
        {
            OnExternalScriptAdded(_scriptsToAdd.Dequeue());
        }
        while (_scriptsToRemove.Count > 0)
        {
            OnExternalScriptRemoved(_scriptsToRemove.Dequeue());
        }
    }
}