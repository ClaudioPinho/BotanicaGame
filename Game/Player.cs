using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace TestMonoGame.Game;

public class Player : Entity
{
    public readonly Camera Camera;

    public float PlayerRunningSpeed = 12f;
    public float PlayerWalkingSpeed = 7f;
    public float PlayerSpeedOnAir = 2f;

    public GameObject ObjectBeingLookedAt;
    public Vector3 ObjectBeingLookedAtNormal;

    private float _playerSpeed;
    private const float _playerCamHeight = 1.7f;

    private bool _wasPreviouslyRightClicking = false;
    private bool _wasPreviouslyLeftClicking = false;

    public Player(string name, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null,
        Transform parent = null) : base(name, true, new Vector3(0.5f, 1.9f, 0.5f), position, rotation, scale, parent)
    {
        Camera = new Camera("Player Camera", Transform.Position + Vector3.Up * _playerCamHeight,
            Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 2, 0));
        Camera.CameraFOV = 45f;
        CollisionOffset = new Vector3(0, CollisionSize.Y / 2, 0f);
        Mass = 1f;
        Restitution = 1f;
        MainGame.GameInstance.AddNewGameObject(Camera);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

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

        if (Keyboard.GetState().IsKeyDown(Keys.Q))
        {
            movementDirection.Y = -1;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.E))
        {
            movementDirection.Y = 1;
        }

        // Normalize movement direction to maintain consistent speed regardless of diagonal movement
        if (movementDirection != Vector3.Zero)
        {
            movementDirection.Normalize();
        }

        Velocity.X = movementDirection.X * _playerSpeed;
        Velocity.Z = movementDirection.Z * _playerSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.Space) && IsOnFloor)
        {
            // Calculate jump velocity based on jump height and gravity
            var jumpVelocity = MathF.Sqrt(2f * JumpHeight * Math.Abs(MainGame.Physics.WorldGravity.Y));

            // Set player's vertical velocity to jump velocity
            Velocity.Y = jumpVelocity;
            // Velocity.Y += 1f;
        }

        // lock camera transform to the player transform
        Camera.Transform.Position = Transform.Position + Vector3.Up * _playerCamHeight;

        if (MainGame.Physics.Raycast(Camera.Transform.Position, Camera.Transform.Forward, out var hit, 10f, Self))
        {
            ObjectBeingLookedAtNormal = hit.HitNormal;
            ObjectBeingLookedAt = hit.GameObjectHit;
        }
        else
        {
            ObjectBeingLookedAtNormal = Vector3.Zero;
            ObjectBeingLookedAt = null;
        }

        if (Mouse.GetState().LeftButton == ButtonState.Pressed && !_wasPreviouslyLeftClicking)
        {
            _wasPreviouslyLeftClicking = true;
            if (ObjectBeingLookedAt != null)
            {
                MainGame.GameInstance.DestroyGameObject(ObjectBeingLookedAt);
                ObjectBeingLookedAt = null;
            }
        }
        else if (Mouse.GetState().LeftButton == ButtonState.Released)
        {
            _wasPreviouslyLeftClicking = false;
        }
        
        if (Mouse.GetState().RightButton == ButtonState.Pressed && !_wasPreviouslyRightClicking)
        {
            _wasPreviouslyRightClicking = true;
            if (ObjectBeingLookedAt != null)
            {
                var newCubePosition = ObjectBeingLookedAt.Transform.Position + ObjectBeingLookedAtNormal;
                var newCube = new PhysicsObject("New Cube", true, false, position: newCubePosition);
                newCube.Model = MainGame.GameInstance.CubeModel;
                // newCube.Texture = MainGame.GameInstance.GrassTexture;
                MainGame.GameInstance.AddNewGameObject(newCube);
            }
        }
        else if (Mouse.GetState().RightButton == ButtonState.Released)
        {
            _wasPreviouslyRightClicking = false;
        }
        

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