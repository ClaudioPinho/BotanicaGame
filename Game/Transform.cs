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
}