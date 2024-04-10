using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class PhysicsObject(
    string name,
    bool isStatic = false,
    bool isAffectedByGravity = true,
    Vector3? collisionSize = null,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null) : MeshObject(name, position, rotation, scale, parent)
{
    public readonly bool IsStatic = isStatic;
    public readonly bool IsAffectedByGravity = isAffectedByGravity;

    public BoundingBox CollisionBox;

    public float Mass = 1f;
    public float Restitution = 1f;

    public Vector3 CollisionSize = collisionSize ?? new Vector3(1, 1, 1);
    public Vector3 CollisionOffset = Vector3.Zero;

    public Vector3 Velocity = new(0, 0, 0);

    public bool DebugDrawCollision = false;

    public virtual void PhysicsTick(float deltaTime)
    {
        UpdateCollisionBox();

        if (!IsStatic && IsAffectedByGravity)
        {
            
        }
        
        // DebugUtils.DrawWireCube(Transform.Position, customCorners: CollisionBox.GetCorners());
        // DebugUtils.DrawDebugAxis(Transform.Position, Transform.Rotation, Transform.Scale);
    }

    public BoundingBox GetCollisionBoxAtPosition(Vector3 position)
    {
        return new BoundingBox(position + CollisionOffset - CollisionSize / 2,
            position + CollisionOffset + CollisionSize / 2);
    }

    private void UpdateCollisionBox()
    {
        CollisionBox.Min = Transform.Position + CollisionOffset - CollisionSize / 2;
        CollisionBox.Max = Transform.Position + CollisionOffset + CollisionSize / 2;
    }
}