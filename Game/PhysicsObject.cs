using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class PhysicsObject(
    string name,
    bool isStatic = false,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null) : MeshObject(name, position, rotation, scale, parent)
{
    public readonly bool IsStatic = isStatic;
}