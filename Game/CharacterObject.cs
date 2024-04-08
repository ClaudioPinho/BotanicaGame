using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class CharacterObject(
    string name,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null) : PhysicsObject(name, false, position, rotation, scale, parent)
{
    // public LinearMotor FrictionMotor { get; private set; }
    // public AngularMotor AngularMovement { get; private set; }
    public float MaxVelocity { get; set; }
}