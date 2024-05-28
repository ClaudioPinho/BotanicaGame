using BotanicaGame.Debug;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Game;

public class PhysicsObject(string id) : MeshObject(id)
{
    public bool IsStatic;
    public bool IsAffectedByGravity = true;
    public bool CanBePushed;

    public BoundingBox CollisionBox;

    public float Mass = 1f;
    public float Restitution = 1f;
    public float Friction = 1f;

    public Vector3 CollisionSize = new(1, 1, 1);
    public Vector3 CollisionOffset = Vector3.Zero;

    public Vector3 Velocity = new(0, 0, 0);

    public bool DebugDrawCollision = false;

    public virtual void PhysicsTick(float deltaTime)
    {
        if (DebugDrawCollision)
        {
            DebugUtils.DrawWireCube(Transform.Position, customCorners: CollisionBox.GetCorners());
            DebugUtils.DrawWirePoint(CollisionBox.Min, color: Color.Red);
            DebugUtils.DrawWirePoint(CollisionBox.Max, color: Color.Green);
            DebugUtils.DrawDebugAxis(Transform.Position, Transform.Rotation, Transform.Scale);
        }
    }

    public void CalculateAABB()
    {
        CollisionBox.Min = Transform.Position + CollisionOffset - CollisionSize / 2;
        CollisionBox.Max = Transform.Position + CollisionOffset + CollisionSize / 2;
    }

    public BoundingBox GetCollisionBoxAtPosition(Vector3 position)
    {
        return new BoundingBox(position + CollisionOffset - CollisionSize / 2,
            position + CollisionOffset + CollisionSize / 2);
    }
}