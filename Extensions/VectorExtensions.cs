using Microsoft.Xna.Framework;

namespace BotanicaGame.Extensions;

public static class VectorExtensions
{
    public static void Set(this ref Vector3 vector, float x, float y, float z)
    {
        vector.X = x;
        vector.Y = y;
        vector.Z = z;
    }
    
    public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
    {
        // Ensure the plane normal is normalized
        planeNormal = Vector3.Normalize(planeNormal);

        // Calculate the projection of the vector onto the plane
        float dotProduct = Vector3.Dot(vector, planeNormal);
        Vector3 projectedVector = vector - dotProduct * planeNormal;

        return projectedVector;
    }

    // public static Vector2 ToVector2(this Vector3 vector)
    // {
    //     return 
    // }
    
}