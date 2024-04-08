using BepuPhysics;
using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class PhysicsObject : MeshObject
{
    public BodyHandle BodyHandle;
    public CollidableReference Support;
    public ConstraintHandle MotionConstraint;

    private Simulation _simulationSpace;
    
    public PhysicsObject(string name, BodyHandle bodyHandle) : base(name)
    {
        BodyHandle = bodyHandle;
    }

    public override void Initialize()
    {
        base.Initialize();

        // RigidBody = new RigidBody(new BoxShape(2f, 2f, 2f))
        // {
        //     Position = Transform.Position.ToJVector(),
        //     Material = new Material
        //     {
        //         KineticFriction = 100f,
        //         StaticFriction = 100f,
        //         Restitution = 1f
        //     },
        //     // EnableDebugDraw = true
        // };
        // GamePhysics.RegisterPhysicsObject(this);
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