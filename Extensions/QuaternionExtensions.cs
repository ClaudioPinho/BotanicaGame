using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Extensions;

public static class QuaternionExtensions
{
    public static Vector3 ToEulerAngles(this Quaternion quaternion)
    {
        var returnedAngles = new Vector3(0f, 0f, 0f);
        
        // roll (x-axis rotation)
        double sinr_cosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        double cosr_cosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        returnedAngles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch (y-axis rotation)
        double sinp = Math.Sqrt(1 + 2 * (quaternion.W * quaternion.Y - quaternion.X * quaternion.Z));
        double cosp = Math.Sqrt(1 - 2 * (quaternion.W * quaternion.Y - quaternion.X * quaternion.Z));
        returnedAngles.Y = (float)(2 * Math.Atan2(sinp, cosp) - Math.PI / 2);

        // yaw (z-axis rotation)
        double siny_cosp = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
        double cosy_cosp = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        returnedAngles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        return returnedAngles;
    } 
}