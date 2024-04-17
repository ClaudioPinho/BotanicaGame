using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class Camera : GameObject
{
    /// <summary>
    /// The current camera being used to render the scene
    /// </summary>
    public static Camera Current { private set; get; }

    public float CameraFOV = 80f;
    public float NearPlane = .01f;
    public float FarPlane = 1000f;

    public float AspectRatio;

    public Matrix ProjectionMatrix;

    public Matrix ViewMatrix;

    public Camera(string name) : base(name)
    {
        Current = this;
        AspectRatio = MainGame.GraphicsDeviceManager.GraphicsDevice.DisplayMode.AspectRatio;
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
            Transform.Position + Vector3.Transform(Vector3.Forward, Transform.Rotation), Vector3.Up);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        UpdateViewMatrix();
    }

}