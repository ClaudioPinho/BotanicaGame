using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class Camera : Component
{
    public float CameraFOV = 80f;
    public float NearPlane = .01f;
    public float FarPlane = 1000f;

    public float AspectRatio;

    public Matrix ProjectionMatrix;

    public Matrix ViewMatrix;

    private Transform _transform;

    public Camera(Transform transform)
    {
        _transform = transform;
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
        ViewMatrix = Matrix.CreateLookAt(_transform.Position,
            _transform.Position + Vector3.Transform(-Vector3.Forward, _transform.Rotation), Vector3.Up);
    }

    public override void Initialize()
    {
        
    }

    public override void Update()
    {
        UpdateViewMatrix();
    }
}