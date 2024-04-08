using Microsoft.Xna.Framework;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class PhysicsObject(
    string name,
    bool isStatic = false,
    Vector3? collisionSize = null,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null) : MeshObject(name, position, rotation, scale, parent)
{
    public readonly bool IsStatic = isStatic;

    public BoundingBox CollisionBox;

    public Vector3 CollisionSize = collisionSize ?? new Vector3(1, 1, 1);
    public Vector3 CollisionOffset = Vector3.Zero;

    public Vector3 Velocity = new(0, 0, 0);

    public bool DebugDrawCollision = false;

    public virtual void EarlyPhysicsTick(float deltaTime)
    {
        CollisionBox.Min = Transform.Position + CollisionOffset - CollisionSize / 2;
        CollisionBox.Max = Transform.Position + CollisionOffset + CollisionSize / 2;
        // Transform.Position += LinearVelocity * deltaTime;
    }
    
    public virtual void LatePhysicsTick(float deltaTime)
    {
        // Transform.Position += LinearVelocity * deltaTime;
    }
    
    public BoundingBox PredictNextBoundingBox(float deltaTime)
    {
        // Calculate the future position of the object
        Vector3 futurePosition = Transform.Position + Velocity * deltaTime;

        // Calculate the future bounding box based on the future position
        Vector3 futureMin = futurePosition + CollisionOffset - CollisionSize / 2;
        Vector3 futureMax = futurePosition + CollisionOffset + CollisionSize / 2;

        // Return the future bounding box
        return new BoundingBox(futureMin, futureMax);
    }
}