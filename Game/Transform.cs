using System;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Game;

public class Transform(Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null)
{
    public Vector3 Position = position ?? Vector3.Zero;
    public Quaternion Rotation = rotation ?? Quaternion.Identity;
    public Vector3 Scale = scale ?? Vector3.One;

    public Matrix WorldMatrix =>
        Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) *
        Matrix.CreateTranslation(Position);

    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);
    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Rotation);

    public override string ToString()
    {
        var roundPosX = MathF.Round(Position.X, 2).ToString("00.00");
        var roundPosY = MathF.Round(Position.Y, 2).ToString("00.00");
        var roundPosZ = MathF.Round(Position.Z, 2).ToString("00.00");
        return $"Position| X:{roundPosX} Y:{roundPosY} Z:{roundPosZ} Rotation| {rotation.ToString()}";
    }
}