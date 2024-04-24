using BotanicaGame.Game;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Physics;

public struct RaycastHit
{
    public GameObject GameObjectHit;
    public Vector3 HitPosition;
    public Vector3 HitNormal;
}