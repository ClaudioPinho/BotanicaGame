using Microsoft.Xna.Framework;
using TestMonoGame.Extensions;

namespace TestMonoGame.Game;

public class PhysicsObject : MeshObject
{
    public enum EBoundingMode
    {
        Box = 0,
        Sphere = 1,
        CustomMesh = 2
    }

    public EBoundingMode BoundingMode = EBoundingMode.Box;

    private Vector3 _currentVelocity = Vector3.Zero;

    public override void Update()
    {
        // physics should update first!
        UpdatePhysics();
        base.Update();
    }

    public virtual void UpdatePhysics()
    {
        
    }

    public BoundingBox GetBoundingBox()
    {
        var boundingBox = new BoundingBox();
        
        foreach (var mesh in Model.Meshes)
        {
            var meshBox = BoundingBox.CreateFromSphere(mesh.BoundingSphere).Transform(Transform.WorldMatrix);
            boundingBox = BoundingBox.CreateMerged(boundingBox, meshBox);
        }

        return boundingBox;
    }
}