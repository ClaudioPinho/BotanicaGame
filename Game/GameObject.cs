using System;
using System.Collections.Generic;
using System.Linq;
using BotanicaGame.Game.SceneManagement;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Game;

public class GameObject(string name) : IDisposable
{
    public bool IsActive = true;
    
    public string Name = name;

    public bool CanOcclude = true;
    
    public readonly Transform Transform = new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    private List<IExternalScript> _externalScripts = [];
    private Queue<IExternalScript> _scriptsToAdd = new();
    private Queue<IExternalScript> _scriptsToRemove = new();
    
    public Scene SceneContext { get; set; }

    private bool _isInitialized;

    public virtual void Initialize()
    {
        foreach (var externalScript in _externalScripts.Where(x => !x.IsInitialized()))
        {
            externalScript.Start(this);
        }
        _isInitialized = true;
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var externalScript in _externalScripts)
        {
            externalScript.Update(deltaTime);
        }
        HandleExternalScriptChanges();
    }

    public void AddExternalScript(IExternalScript externalScript) => _scriptsToAdd.Enqueue(externalScript);

    public void RemoveExternalScript(IExternalScript externalScript) => _scriptsToRemove.Enqueue(externalScript);
    
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    
    protected virtual void OnExternalScriptAdded(IExternalScript externalScript)
    {
        if (_externalScripts.Contains(externalScript)) return;
        _externalScripts.Add(externalScript);
        if (_isInitialized)
            externalScript.Start(this);
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