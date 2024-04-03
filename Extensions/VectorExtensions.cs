using Jitter2.LinearMath;
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
    
    public static JVector ToJVector(this Vector3 vector)
    {
        return new JVector(vector.X, vector.Y, vector.Z);
    }

    public static Vector3 ToVector3(this JVector vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    public static void FromJVector(this ref Vector3 vector, JVector jVector)
    {
        vector.X = jVector.X;
        vector.Y = jVector.Y;
        vector.Z = jVector.Z;
    }

}