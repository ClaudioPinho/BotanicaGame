using Jitter2;
using Jitter2.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class PhysicsObject : MeshObject
{
    public RigidBody RigidBody { get; private set; }
    public World PhysicsWorld { get; set; }

    public override void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null,
        Vector3? objectScale = null)
    {
        base.Initialize(objectPosition, objectRotation, objectScale);
        GamePhysics.RegisterPhysicsObject(this);
        RigidBody = PhysicsWorld.CreateRigidBody();
        DebugUtils.PrintMessage(Transform.Position.ToString());
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Transform.Position.FromJVector(RigidBody.Position);
        // Transform.Rotation.FromJQuaternion(JQuaternion.CreateFromMatrix(RigidBody.Orientation));
    }

    public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
    {
        base.Draw(graphicsDevice, gameTime);
        RigidBody.DebugDraw(DebugUtils.CollisionDrawer);
    }

    public override void Dispose()
    {
        GamePhysics.UnRegisterPhysicsObject(this);
        base.Dispose();
    }
}