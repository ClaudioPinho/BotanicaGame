using System;

namespace TestMonoGame.Game;

public class GameObject : IDisposable
{
    public readonly Transform Transform = new();

    public virtual void Initialize()
    {
        
    }

    public virtual void Update()
    {
    }

    public virtual void Dispose()
    {
        
    }
}