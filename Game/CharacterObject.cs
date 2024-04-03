using System;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;

namespace TestMonoGame.Game;

public class CharacterObject : PhysicsObject
{
    public LinearMotor FrictionMotor { get; private set; }
    public AngularMotor AngularMovement { get; private set; }

    public override void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null, Vector3? objectScale = null)
    {
        base.Initialize(objectPosition, objectRotation, objectScale);

        var characterShape = new CapsuleShape(0.5f, 1.0f);
        RigidBody.AddShape(characterShape);
        RigidBody.Position = Transform.Position.ToJVector();

        RigidBody.Damping = (0, 0);
        
        RigidBody.DeactivationTime = TimeSpan.MaxValue;
        
        // Add two arms - to be able to visually tell how the player is orientated
        var arm1 = new TransformedShape(new BoxShape(0.2f, 0.8f, 0.2f), new JVector(+0.5f, 0.3f, 0));
        var arm2 = new TransformedShape(new BoxShape(0.2f, 0.8f, 0.2f), new JVector(-0.5f, 0.3f, 0));

        // Add the shapes without recalculating mass and inertia, we take mass and inertia from the capsule
        // shape we added before.
        RigidBody.AddShape(arm1, false);
        RigidBody.AddShape(arm2, false);
        
        // Make the capsule stand upright, but able to rotate 360 degrees.
        var ur = PhysicsWorld.CreateConstraint<HingeAngle>(RigidBody, PhysicsWorld.NullBody);
        ur.Initialize(JVector.UnitY, AngularLimit.Full);
        // DebugUtils.PrintMessage("constraint created!");
        
        
        // Add a "motor" to the body. The motor target velocity is zero.
        // This acts like friction and stops the player.
        // FrictionMotor = PhysicsWorld.CreateConstraint<LinearMotor>(RigidBody, PhysicsWorld.NullBody);
        // FrictionMotor.Initialize(JVector.UnitZ, JVector.UnitX);
        // FrictionMotor.MaximumForce = 10;
        //
        // // An angular motor for turning.
        // AngularMovement = PhysicsWorld.CreateConstraint<AngularMotor>(RigidBody, PhysicsWorld.NullBody);
        // AngularMovement.Initialize(JVector.UnitY, JVector.UnitY);
        // AngularMovement.MaximumForce = 1000;
    }

    // private bool IsOnFloor()
    // {
    //     
    // }
}