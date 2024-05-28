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

    public readonly Transform Transform = new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    public GameObject Parent
    {
        get => _parent;
        set
        {
            if (_parent == this)
            {
                DebugUtils.PrintError($"Game object with id '{Id}' is trying to parent to itself!", this);
            }
            else if (value != _parent)
            {
                _parentIsDirty = true;
                _parent = value;
            }
        }
    }

    public Scene SceneContext { get; set; }

    private readonly List<IExternalScript> _externalScripts = [];
    private readonly Queue<IExternalScript> _scriptsToAdd = new();
    private readonly Queue<IExternalScript> _scriptsToRemove = new();

    private bool _parentIsDirty;
    private bool _isInitialized;
    private GameObject _parent;

    public void AddExternalScript(IExternalScript externalScript) => _scriptsToAdd.Enqueue(externalScript);

    public void RemoveExternalScript(IExternalScript externalScript) => _scriptsToRemove.Enqueue(externalScript);

    public T FindTypeInParents<T>()
    {
        if (this is T foundType)
            return foundType;
        return Parent != null ? Parent.FindTypeInParents<T>() : default;
    }

    public T FindTypeInParent<T>()
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
        if (_parentIsDirty)
        {
            UpdateParent(_parent);
            _parentIsDirty = false;
        }
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

    protected virtual void UpdateParent(GameObject parentObject)
    {
        
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