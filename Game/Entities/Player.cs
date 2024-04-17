using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using TestMonoGame.Extensions;
using TestMonoGame.Physics;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace TestMonoGame.Game.Entities;

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

    public readonly AudioListener AudioListener;

    private float _footstepTimer = 0.0f;
    private float _footstepRate = 0.3f; // Adjust as needed
    private float _walkingThreshold = 0.1f; // Adjust as needed

    private RaycastHit _cameraHit;

    public Player(string name) : base(name)
    {
        AudioListener = new AudioListener();
        
        Camera = new Camera("Player Camera");
        Camera.Transform.Position = Transform.Position + Vector3.Up * _playerCamHeight;
        Camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 2, 0);
        Camera.CameraFOV = 45f;
        
        CollisionSize = new Vector3(0.5f, 1.9f, 0.5f);
        CollisionOffset = new Vector3(0, CollisionSize.Y / 2, 0f);
    }

    public override void Initialize()
    {
        base.Initialize();
        
        SceneContext.AddNewGameObject(Camera);
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        UpdateAudioListenerState();

        // re-position the player back at origin if they fall
        if (Transform.Position.Y < -100)
        {
            Transform.Position.Set(0, 10, 0);
            Velocity = Vector3.Zero;
        }

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
        // Velocity.Y = movementDirection.Y * _playerSpeed;
        Velocity.Z = movementDirection.Z * _playerSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            TryJump();
        }

        // lock camera transform to the player transform
        Camera.Transform.Position = Transform.Position + Vector3.Up * _playerCamHeight;

        if (MainGame.Physics.Raycast(Camera.Transform.Position, Camera.Transform.Forward, ref _cameraHit, 10f,
                this))
        {
            ObjectBeingLookedAtNormal = _cameraHit.HitNormal;
            ObjectBeingLookedAt = _cameraHit.GameObjectHit;
            DebugUtils.DrawWireCube(ObjectBeingLookedAt.Transform.Position, color: Color.Black);
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
                // MainGame.GameInstance.DestroyGameObject(ObjectBeingLookedAt);
                // MainGame.GameInstance.RemoveBlockSfx?.Play();
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
                // var newCubePosition = ObjectBeingLookedAt.Transform.Position + ObjectBeingLookedAtNormal;
                // var newCube = new PhysicsObject("New Cube", true, false, position: newCubePosition);
                // newCube.Model = MainGame.GameInstance.CubeModel;
                // newCube.Texture = MainGame.GameInstance.GrassTexture;
                // MainGame.GameInstance.AddNewGameObject(newCube);
                // MainGame.GameInstance.PlaceBlockSfx?.Play();
            }
        }
        else if (Mouse.GetState().RightButton == ButtonState.Released)
        {
            _wasPreviouslyRightClicking = false;
        }

        if (Velocity.Length() > _walkingThreshold)
        {
            _footstepTimer += deltaTime;
            if (_footstepTimer >= _footstepRate)
            {
                // MainGame.GameInstance.WalkSfx?.Play();
                // if (MainGame.GameInstance.WalkSfx != null)
                // {
                //     MainGame.GameInstance.Play3DAudio(AudioEmitter, MainGame.GameInstance.WalkSfx, 10f, 1f);
                // }

                _footstepTimer = 0f;
            }
        }

        if (MainGame.GameInstance.IsActive)
            UpdateInput();
    }

    public override void OnReceivedDamage(int damageReceived)
    {
        base.OnReceivedDamage(damageReceived);
        // MainGame.GameInstance.FallSfx.Play();
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

    private void UpdateAudioListenerState()
    {
        AudioListener.Position = Camera.Transform.Position;
        AudioListener.Forward = Camera.Transform.Forward;
        AudioListener.Up = Camera.Transform.Up;
        AudioListener.Velocity = Velocity;
    }
}