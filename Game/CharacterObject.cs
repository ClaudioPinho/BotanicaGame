using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class CharacterObject(
    string name,
    Vector3? collisionSize = null,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null) : PhysicsObject(name, false, collisionSize, position, rotation, scale, parent)
{
    // public float MaxVelocity { get; set; }
}