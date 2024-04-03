using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class PhysicsObject : MeshObject
{
    public RigidBody RigidBody { get; private set; }
    
    public override void Initialize()
    {
        base.Initialize();
        DebugUtils.PrintMessage(Transform.Position.ToString());
        RigidBody = GamePhysics.World.CreateRigidBody();
        SetPositionAndRotation(Transform.Position, Transform.Rotation);
        GamePhysics.RegisterPhysicsObject(this);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Transform.Position.FromJVector(RigidBody.Position);
        Transform.Rotation.FromJQuaternion(JQuaternion.CreateFromMatrix(RigidBody.Orientation));
    }

    public override void Dispose()
    {
        GamePhysics.UnRegisterPhysicsObject(this);
        base.Dispose();
    }

    public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        Transform.Position = position;
        Transform.Rotation = rotation;
        RigidBody.Position = position.ToJVector();
        RigidBody.Orientation = JMatrix.CreateFromQuaternion(rotation.ToJQuaternion());
    }
}