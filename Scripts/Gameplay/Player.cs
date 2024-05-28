using System;
using BotanicaGame.Debug;
using BotanicaGame.Extensions;
using BotanicaGame.Game;
using BotanicaGame.Game.Entities;
using BotanicaGame.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Scripts.Gameplay;

public class Player : IExternalScript
{
    public static Player LocalPlayer { get; private set; }
    
    public Entity Entity { get; private set; }
    
    public int MaxOxygenLevel { get; private set; } = 25;
    public int OxygenLevel { get; private set; }
    
    public Camera Camera;

    public GameObject ObjectBeingLookedAt;
    public Vector3 ObjectBeingLookedAtNormal;
    
    public AudioListener AudioListener;
    
    private const float _playerCamHeight = 1.7f;

    private bool _wasPreviouslyRightClicking;
    private bool _wasPreviouslyLeftClicking;
    
    // oxygen related data
    private float _oxygenConsumptionPerSec = 0.25f;
    private float _oxygenConsumed;
    
    // movement related data
    private bool _isRunning;
    private float _playerSpeed;
    private float _footstepTimer;
    private float _footstepRate = 0.3f; // Adjust as needed
    private float _walkingThreshold = 0.1f; // Adjust as needed

    private RaycastHit _cameraHit;

    public bool IsInitialized() => true;

    public void Start(GameObject gameObjectContext)
    {
        Entity = gameObjectContext as Entity;

        if (Entity == null)
        {
            DebugUtils.PrintError("Player script was placed on the wrong game object!");
            return;
        }
        
        AudioListener = new AudioListener();
        
        Camera = new Camera("Player Camera");
        Camera.Transform.Position = Entity.Transform.Position + Vector3.Up * _playerCamHeight;
        Camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 2, 0);
        Camera.CameraFOV = 45f;
        
        gameObjectContext.SceneContext.AddNewGameObject(Camera);
        
        Entity.CollisionSize = new Vector3(0.5f, 1.9f, 0.5f);
        Entity.CollisionOffset = new Vector3(0, Entity.CollisionSize.Y / 2, 0f);

        Entity.OnDamageReceived += OnReceivedDamage;

        OxygenLevel = MaxOxygenLevel;
        
        LocalPlayer = this;
    }

    public void Update(float deltaTime)
    {
        if (Entity == null) return;
        
        UpdateAudioListenerState();

        // re-position the player back at origin if they fall
        if (Entity.Transform.Position.Y < -100)
        {
            Entity.Transform.Position.Set(0, 10, 0);
            Entity.Velocity = Vector3.Zero;
        }

        _isRunning = Keyboard.GetState().IsKeyDown(Keys.LeftShift);
        _playerSpeed = _isRunning ? Entity.RunningSpeed : Entity.WalkingSpeed;

        if (MainGame.LockCursor)
        {
            UpdateMovement();
        }

        // lock camera transform to the player transform
        Camera.Transform.Position = Entity.Transform.Position + Vector3.Up * _playerCamHeight;

        if (MainGame.Physics.Raycast(Camera.Transform.Position, Camera.Transform.Forward, ref _cameraHit, 10f,
            Entity))
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

        if (Entity.Velocity.Length() > _walkingThreshold)
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

        // oxygen logic, doubles the oxygen consumption when running
        _oxygenConsumed += _oxygenConsumptionPerSec * deltaTime * (_isRunning ? 2f : 1f);
        if (_oxygenConsumed > 1f)
        {
            if (OxygenLevel > 0)
            {
                OxygenLevel--;
            }
            else
            {
                Entity.GiveDamage(1);
            }
            _oxygenConsumed -= 1f;
        }
        
        if (MainGame.GameInstance.IsActive && MainGame.LockCursor)
            UpdateInput();
    }

    public void OnReceivedDamage(int damageReceived)
    {
        // MainGame.GameInstance.FallSfx.Play();
    }

    private float yaw = 0f;
    private float pitch = 0f;

    private const float _maxPitch = MathF.PI / 2 - 0.01f; // Just shy of 90 degrees
    private const float _minPitch = -_maxPitch;

    private void UpdateMovement()
    {
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

        Entity.Velocity.X = movementDirection.X * _playerSpeed;
        // Velocity.Y = movementDirection.Y * _playerSpeed;
        Entity.Velocity.Z = movementDirection.Z * _playerSpeed;

        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            Entity.TryJump();
        }
    }
    
    private void UpdateInput()
    {
        
        var delta = Mouse.GetState().Position - MainGame.WindowCenter;

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
        AudioListener.Velocity = Entity.Velocity;
    }
}