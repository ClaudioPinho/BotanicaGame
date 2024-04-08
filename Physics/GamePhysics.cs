using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

public class GamePhysics(Vector3? worldGravity = null)
{
    public readonly List<PhysicsObject> PhysicsObjects = [];
    
    private Vector3 _worldGravity = worldGravity ?? new Vector3(0, -9.81f, 0);

    public void AddPhysicsObject(PhysicsObject physicsObject)
    {
        if (PhysicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to add a physics object that is already registered", physicsObject);
            return;
        }
        
        PhysicsObjects.Add(physicsObject);
    }

    public void RemovePhysicsObject(PhysicsObject physicsObject)
    {
        if (!PhysicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to remove a physics object that is not registered", physicsObject);
            return;
        }

        PhysicsObjects.Remove(physicsObject);
    }
    
    public void UpdatePhysics(GameTime gameTime)
    {
        
    }

    /// <summary>
    /// Checks if physics object 1 and object 2 intersect with one another
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public bool IsIntersecting(PhysicsObject obj1, PhysicsObject obj2)
    {
        return false;
        // return obj1.CheckCollision(obj2.GetBoundingBox());
    }
}