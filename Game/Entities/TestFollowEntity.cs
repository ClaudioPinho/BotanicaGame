using Microsoft.Xna.Framework;

namespace TestMonoGame.Game;

public class TestFollowEntity(
    string name,
    bool isAffectedByGravity = true,
    Vector3? collisionSize = null,
    Vector3? position = null,
    Quaternion? rotation = null,
    Vector3? scale = null,
    Transform parent = null)
    : Entity(name, isAffectedByGravity, collisionSize, position, rotation, scale, parent)
{
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
    }
}