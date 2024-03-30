using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Extensions;

public static class QuaternionExtensions
{
    public static float Yaw(this Quaternion quaternion)
    {
        return (float)Math.Atan2(2 * (quaternion.Y * quaternion.Z + quaternion.W * quaternion.X),
            quaternion.W * quaternion.W - quaternion.X * quaternion.X - quaternion.Y * quaternion.Y +
            quaternion.Z * quaternion.Z);
    }

    public static float Pitch(this Quaternion quaternion)
    {
        return (float)Math.Asin(-2 * (quaternion.X * quaternion.Z - quaternion.W * quaternion.Y));
    }

    public static float Roll(this Quaternion quaternion)
    {
        return (float)Math.Atan2(2 * (quaternion.X * quaternion.Y + quaternion.W * quaternion.Z),
            quaternion.W * quaternion.W + quaternion.X * quaternion.X - quaternion.Y * quaternion.Y -
            quaternion.Z * quaternion.Z);
    }

    public static void SetYaw(this ref Quaternion quaternion, float yaw)
    {
        quaternion = Quaternion.CreateFromYawPitchRoll(yaw, quaternion.Pitch(), quaternion.Roll());
    }

    public static void SetPitch(this ref Quaternion quaternion, float pitch)
    {
        quaternion = Quaternion.CreateFromYawPitchRoll(quaternion.Yaw(), pitch, quaternion.Roll());
    }

    public static void SetRoll(this ref Quaternion quaternion, float roll)
    {
        quaternion = Quaternion.CreateFromYawPitchRoll(quaternion.Yaw(), quaternion.Pitch(), roll);
    }

    public static Vector3 ToEuler(this Quaternion quaternion, bool degrees = true)
    {
        // Roll (x-axis rotation)
        double sinr_cosp = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        double cosr_cosp = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        var roll = Math.Atan2(sinr_cosp, cosr_cosp);

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
        var yaw = Math.Atan2(siny_cosp, cosy_cosp);

        if (degrees)
        {
            return new Vector3(MathHelper.ToDegrees((float)roll), MathHelper.ToDegrees((float)pitch),
                MathHelper.ToDegrees((float)yaw));
        }

        return new Vector3((float)roll, (float)pitch, (float)yaw);
    }

    // public static Quaternion FromEuler(this Quaternion quaternion, float x, float y, float z, bool degrees = true)
    // {
    //     if (degrees)
    //     {
    //         x = MathHelper.ToRadians(x);
    //         y = MathHelper.ToRadians(y);
    //         z = MathHelper.ToRadians(z);
    //     }
    //
    //     // Convert Euler angles to radians
    //     var yaw = y * 0.5f;
    //     var pitch = x * 0.5f;
    //     var roll = z * 0.5f;
    //
    //     // Calculate sin and cos values
    //     var sy = (float)Math.Sin(yaw);
    //     var cy = (float)Math.Cos(yaw);
    //     var sp = (float)Math.Sin(pitch);
    //     var cp = (float)Math.Cos(pitch);
    //     var sr = (float)Math.Sin(roll);
    //     var cr = (float)Math.Cos(roll);
    //
    //     // Calculate quaternion components
    //     var w = cr * cp * cy + sr * sp * sy;
    //     var xComponent = sr * cp * cy - cr * sp * sy;
    //     var yComponent = cr * sp * cy + sr * cp * sy;
    //     var zComponent = cr * cp * sy - sr * sp * cy;
    //
    //     quaternion.X = xComponent;
    //     quaternion.Y = yComponent;
    //     quaternion.Z = zComponent;
    //     quaternion.W = w;
    //
    //     return quaternion;
    // }

    public static void SetEulerAngles(this ref Quaternion quaternion, float yaw, float pitch, float roll,
        bool isRadians = false)
    {
        
        if (isRadians)
            quaternion = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        else
            quaternion = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch),
                MathHelper.ToRadians(roll));
    }
}