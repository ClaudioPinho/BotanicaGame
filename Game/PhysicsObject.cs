using Jitter;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
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

    public bool UsePhysicsRotation { get; set; } = true;

    public override void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null,
        Vector3? objectScale = null)
    {
        base.Initialize(objectPosition, objectRotation, objectScale);
        RigidBody = new RigidBody(new BoxShape(1f, 1f, 1f))
        {
            Position = Transform.Position.ToJVector(),
            Material = new Material
            {
                KineticFriction = 1f,
                StaticFriction = 1f,
                Restitution = 1f
            },
            EnableDebugDraw = true
        };
        GamePhysics.RegisterPhysicsObject(this);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        Transform.Position.FromJVector(RigidBody.Position);
        if (UsePhysicsRotation)
            Transform.Rotation.FromJQuaternion(JQuaternion.CreateFromMatrix(RigidBody.Orientation));
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