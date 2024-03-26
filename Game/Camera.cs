using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game;

public class Camera : GameObject
{
    public float CameraFOV = 80f;
    public float NearPlane = 1f;
    public float FarPlane = 1000f;

    public float AspectRatio;

    public Matrix ProjectionMatrix;

    public Matrix ViewMatrix;

    public Camera(GraphicsDevice graphics)
    {
        AspectRatio = graphics.DisplayMode.AspectRatio;
        UpdateProjectionMatrix();
        UpdateViewMatrix();
    }

    public void UpdateProjectionMatrix()
    {
        ProjectionMatrix =
            Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(CameraFOV), AspectRatio, NearPlane, FarPlane);
    }

    public void UpdateViewMatrix()
    {
        ViewMatrix = Matrix.CreateLookAt(Transform.Position,
            Transform.Position + Vector3.Transform(-Vector3.Forward, Transform.Rotation), Vector3.Up);
    }

    public override void Update()
    {
        base.Update();
        UpdateViewMatrix();
    }
}