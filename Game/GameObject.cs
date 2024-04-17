using System;
using Microsoft.Xna.Framework;
using TestMonoGame.Game.SceneManagement;

namespace TestMonoGame.Game;

public class GameObject(string name) : IDisposable
{
    public string Name = name;

    public bool CanOcclude = true;
    
    public readonly Transform Transform = new(Vector3.Zero, Quaternion.Identity, Vector3.One);
    
    public Scene SceneContext { get; set; }

    public virtual void Initialize()
    {
    }

    public virtual void Update(float deltaTime)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}