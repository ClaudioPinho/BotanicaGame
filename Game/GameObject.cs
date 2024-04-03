using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class GameObject : IDisposable
{
    public string Id { get; set; }
    public string Name { get; set; }
    
    public readonly Transform Transform = new Transform();

    public virtual void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null,
        Vector3? objectScale = null)
    {
        SetPositionAndRotation(objectPosition ?? Vector3.Zero, objectRotation ?? Quaternion.Identity);
        Transform.Scale = objectScale ?? Vector3.One;
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        Transform.Position = position;
        Transform.Rotation = rotation;
    }
}