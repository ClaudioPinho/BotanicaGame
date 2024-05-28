using System;
using BotanicaGame.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace BotanicaGame.Game.Entities;

public class Entity : PhysicsObject
{
    private const float FallingDamageRatio = 0.1f;
    private const float CriticalFallSpeed = 15f;

    public float RunningSpeed = 12f;
    public float WalkingSpeed = 7f;
    public float SpeedOnAir = 2f;

    public int MaxHealth = 10;
    public int Health;
    public float JumpHeight = 1.5f;

    // AUDIO
    public readonly AudioEmitter AudioEmitter = new();

    public SoundEffect JumpSfx;
    public SoundEffect FallSfx;

    public Action<int> OnDamageReceived;

    public bool IsOnFloor { get; private set; } = true;

    public bool WasPreviouslyFalling { get; private set; }
    // public float MaxVelocity { get; set; }

    public Entity(string id) : base(id)
    {
        IsStatic = false;
        IsAffectedByGravity = true;

        Health = MaxHealth;
        
        Mass = 1f;
        Restitution = 1f;
        
        AudioEmitter = new AudioEmitter();
    }

    public override void Update(float deltaTime)
    {
        UpdateAudioEmitterState();

        IsOnFloor = MainGame.Physics.BoxcastHitCheck(Transform.Position, Vector3.Down,
            CollisionSize * 0.5f - (CollisionSize * 0.5f * Vector3.Up - Vector3.Up * 0.1f), 0.1f, this);

        // attempt to give fall damage
        if (IsOnFloor && WasPreviouslyFalling)
        {
            // assuming a Y vector for falling damage
            GiveDamage(CalculateFallDamage(Velocity.Y));
        }

        WasPreviouslyFalling = !IsOnFloor;
        
        base.Update(deltaTime);
    }

    public virtual bool TryJump()
    {
        if (!IsOnFloor) return false;
        Velocity.Y = MathF.Sqrt(2f * JumpHeight * Math.Abs(MainGame.Physics.WorldGravity.Y));
        JumpSfx?.Play();
        return true;
    }

    public virtual void GiveDamage(int damage)
    {
        if (damage <= 0) return;
        Health -= damage;
        OnReceivedDamage(damage);
    }

    public virtual void OnReceivedDamage(int damageReceived)
    {
        try
        {
            OnDamageReceived?.Invoke(damageReceived);
        }
        catch (Exception e)
        {
            DebugUtils.PrintException(e);
        }
    }

    private static int CalculateFallDamage(float fallingSpeed)
    {
        if (Math.Abs(fallingSpeed) < CriticalFallSpeed) return 0;
        return (int)(Math.Abs(fallingSpeed) * FallingDamageRatio);
    }

    private void UpdateAudioEmitterState()
    {
        AudioEmitter.Position = Transform.Position;
        AudioEmitter.Forward = Transform.Forward;
        AudioEmitter.Up = Transform.Up;
        AudioEmitter.Velocity = Velocity;
    }
}