using Microsoft.Xna.Framework;

namespace TestMonoGame.Extensions;

public static class VectorExtensions
{
    public static Vector3 ProjectOntoPlane(this Vector3 vector, Vector3 planeNormal)
    {
        // Ensure the plane normal is normalized
        planeNormal = Vector3.Normalize(planeNormal);

        // Calculate the projection of the vector onto the plane
        float dotProduct = Vector3.Dot(vector, planeNormal);
        Vector3 projectedVector = vector - dotProduct * planeNormal;

        return projectedVector;
    }
}