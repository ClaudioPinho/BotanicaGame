using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace TestMonoGame.Game;

public class Entity : PhysicsObject
{
    private const float FallingDamageRatio = 0.1f;
    private const float CriticalFallSpeed = 15f;

    public int Health = 10;
    public float JumpHeight = 1.5f;

    // AUDIO
    public readonly AudioEmitter AudioEmitter;

    public SoundEffect JumpSfx;
    public SoundEffect FallSfx;

    public bool IsOnFloor { get; private set; } = true;

    public bool WasPreviouslyFalling { get; private set; }
    // public float MaxVelocity { get; set; }

    public Entity(string name,
        bool isAffectedByGravity = true,
        Vector3? collisionSize = null,
        Vector3? position = null,
        Quaternion? rotation = null,
        Vector3? scale = null,
        Transform parent = null) : base(name, false, isAffectedByGravity, collisionSize, position, rotation, scale,
        parent)
    {
        AudioEmitter = new AudioEmitter();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

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
    }

    protected virtual bool TryJump()
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