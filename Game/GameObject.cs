using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class GameObject(string name) : IDisposable
{
    public string Name = name;
    
    public readonly Transform Transform = new()
    {
        Position = Vector3.Zero,
        Rotation = Quaternion.Identity,
        Scale = Vector3.One
    };

    public virtual void Initialize()
    {
        
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}