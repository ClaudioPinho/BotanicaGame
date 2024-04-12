using Microsoft.Xna.Framework;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

public struct RaycastHit
{
    public GameObject GameObjectHit;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
}