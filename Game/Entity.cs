using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TestMonoGame.Physics;

namespace TestMonoGame.Game;

public class Entity(
    string name,
    bool isAffectedByGravity = true,
    Vector3? collisionSize = null,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null)
    : PhysicsObject(name, false, isAffectedByGravity, collisionSize, position, rotation, scale, parent)
{
    // public float MaxVelocity { get; set; }
    public float JumpHeight = 1.5f;

    public bool IsOnFloor = true;

    public List<PhysicsObject> Self;

    public override void Initialize()
    {
        base.Initialize();
        Self = [this];
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        // todo: this might be computationally expensive, need to check other ways of doing it
        IsOnFloor = MainGame.Physics.RaycastHitCheck(Transform.Position + Vector3.Up * 0.1f, Vector3.Down, 0.2f, Self);
    }
}