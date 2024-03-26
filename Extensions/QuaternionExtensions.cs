using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Extensions;

public static class QuaternionExtensions
{
    
    public static Vector3 ToEulerAngles(this Quaternion quaternion)
    {
        // Roll (x-axis rotation)
        double sinr_cosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        double cosr_cosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        double roll = Math.Atan2(sinr_cosp, cosr_cosp);

        // Pitch (y-axis rotation)
        double sinp = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
        double pitch;
        if (Math.Abs(sinp) >= 1)
            pitch = Math.PI / 2 * Math.Sign(sinp); // use 90 degrees if out of range
        else
            pitch = Math.Asin(sinp);

        // Yaw (z-axis rotation)
        double siny_cosp = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
        double cosy_cosp = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        double yaw = Math.Atan2(siny_cosp, cosy_cosp);

        return new Vector3((float)roll, (float)pitch, (float)yaw);
    } 
}