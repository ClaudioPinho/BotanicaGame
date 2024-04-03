using System;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace TestMonoGame.Game;

// https://github.com/notgiven688/jitterphysics2/blob/main/src/JitterDemo/Demos/Player/Player.cs
// https://jitterphysics.com/docs/demo/jitterdemo
public class Player : CharacterObject
{
    public Camera Camera { get; private set; }

    public float PlayerRunningSpeed = 20f;
    public float PlayerWalkingSpeed = 15f;
    
    private float _playerSpeed;
    private const float _playerCamHeight = 1.8f;

    public override void Initialize(Vector3? objectPosition = null, Quaternion? objectRotation = null,
        Vector3? objectScale = null)
    {
        Camera = MainGame.GameInstance.CreateNewGameObject<Camera>();
        Camera.CameraFOV = 45f;
        Camera.Transform.Rotation.SetEulerAngles(0f, 90f, 0f);
        base.Initialize(objectPosition, objectRotation, objectScale);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _playerSpeed = Keyboard.GetState().IsKeyDown(Keys.LeftShift) ? PlayerRunningSpeed : PlayerWalkingSpeed;

        // Get forward and right vectors on the XZ plane
        var forwardXZ = new Vector3(Camera.Transform.Forward.X, 0f, Camera.Transform.Forward.Z);
        var rightXZ = new Vector3(Camera.Transform.Right.X, 0f, Camera.Transform.Right.Z);
        // var forwardXZ = Vector3.UnitZ;
        // var rightXZ = Vector3.UnitX;
        forwardXZ.Normalize(); // Ensure the vectors have unit length
        rightXZ.Normalize();

        // Calculate movement direction based on keyboard input
        var movementDirection = Vector3.Zero;

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            movementDirection += forwardXZ;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            movementDirection -= forwardXZ;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            movementDirection -= rightXZ;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            movementDirection += rightXZ;
        }

        // Normalize movement direction to maintain consistent speed regardless of diagonal movement
        if (movementDirection != Vector3.Zero)
        {
            movementDirection.Normalize();
        }

        // var velocityChange = movementDirection.ToJVector() * _playerSpeed - RigidBody.LinearVelocity;
        // RigidBody.ApplyImpulse(velocityChange);
        var desiredVelocity = movementDirection.ToJVector() * _playerSpeed;
        RigidBody.LinearVelocity = new JVector(desiredVelocity.X, RigidBody.LinearVelocity.Y, desiredVelocity.Z);

        if (Keyboard.GetState().IsKeyDown(Keys.Space) && Transform.Position.Y < 2.2f)
        {
            RigidBody.ApplyImpulse(JVector.Up * 100f);
        }

        // lock camera transform to the player transform
        Camera.Transform.Position = Transform.Position + Vector3.Up * _playerCamHeight;

        if (MainGame.GameInstance.IsActive)
            UpdateInput();
    }

    private float yaw = 0f;
    private float pitch = 0f;

    private const float _maxPitch = MathF.PI / 2 - 0.01f; // Just shy of 90 degrees
    private const float _minPitch = -_maxPitch;

    private void UpdateInput()
    {
        var center = new Point(MainGame.GameInstance.Window.ClientBounds.Width / 2,
            MainGame.GameInstance.Window.ClientBounds.Height / 2);
        var delta = Mouse.GetState().Position - center;

        // Reset mouse position to center of window
        Mouse.SetPosition(center.X, center.Y);

        // Adjust camera orientation based on mouse movement
        var sensitivity = 0.005f;

        // Calculate rotation angles based on mouse movement
        yaw -= delta.X * sensitivity;
        pitch -= delta.Y * sensitivity;

        // Clamp the pitch angle to prevent camera inversion
        pitch = MathHelper.Clamp(pitch, _minPitch, _maxPitch);

        Camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0f);
        // Transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw - MathF.PI / 2, 0f, 0f);
    }
}