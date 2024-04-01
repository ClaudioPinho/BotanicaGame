using Microsoft.Xna.Framework;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

public static class Physics
{
    /// <summary>
    /// Checks if physics object 1 and object 2 intersect with one another
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool IsIntersecting(PhysicsObject obj1, PhysicsObject obj2)
    {
        return obj1.GetBoundingBox().Intersects(obj2.GetBoundingBox());
    }
}