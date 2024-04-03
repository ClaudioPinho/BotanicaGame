using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.Dynamics.Constraints;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using FixedAngle = Jitter.Dynamics.Constraints.SingleBody.FixedAngle;

namespace TestMonoGame.Game;

public class CharacterObject : PhysicsObject
{
    // public LinearMotor FrictionMotor { get; private set; }
    // public AngularMotor AngularMovement { get; private set; }
    public float MaxVelocity { get; set; }

    public override void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null, Vector3? objectScale = null)
    {
        base.Initialize(objectPosition, objectRotation, objectScale);

        MaxVelocity = 10f;
        
        var characterShape = new CapsuleShape(2f, 1.0f);
        RigidBody.Shape = characterShape;
        RigidBody.Shape.UpdateShape();

        var constraint = new FixedAngle(RigidBody);
        PhysicsWorld.AddConstraint(constraint);

        RigidBody.Damping = RigidBody.DampingType.None;
        RigidBody.Material.KineticFriction = 0f;
        RigidBody.Material.StaticFriction = 0f;
        RigidBody.Material.Restitution = 0f;
        RigidBody.AllowDeactivation = false;

        UsePhysicsRotation = false;

    }

}