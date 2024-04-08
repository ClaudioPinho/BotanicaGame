using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class GameObject : IDisposable
{
    public string Name;

    public readonly Transform Transform;

    public GameObject(string name, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null,
        Transform parent = null)
    {
        Name = name;
        Transform = new Transform(position, rotation, scale);
    }

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