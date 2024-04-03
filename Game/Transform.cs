using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class Transform
{
    public Vector3 Position = new(0f, 0f, 0f);
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = new(1f, 1f, 1f);

    public Matrix WorldMatrix => Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) *
                                 Matrix.CreateTranslation(Position);

    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Rotation);
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);

    public override string ToString()
    {
        var roundPosX = MathF.Round(Position.X, 2).ToString("00.00");
        var roundPosY = MathF.Round(Position.Y, 2).ToString("00.00");
        var roundPosZ = MathF.Round(Position.Z, 2).ToString("00.00");
        return $"Position| X:{roundPosX} Y:{roundPosY} Z:{roundPosZ}";
    }
}