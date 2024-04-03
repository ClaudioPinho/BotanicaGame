using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class GameObject : IDisposable
{
    public readonly Transform Transform = new();

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