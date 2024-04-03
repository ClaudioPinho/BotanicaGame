using System.Collections.Generic;
using Jitter;
using Jitter.Collision;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

// https://jitterphysics.com/docs/quickstart/hello-world
public static class GamePhysics
{
    public const bool IsMultithread = true;
    
    public static World World { get; private set; }

    private static List<PhysicsObject> _physicsObjects;

    public static void Initialize()
    {
        // ThreadPool.Instance.ChangeThreadCount(1);
        _physicsObjects = new List<PhysicsObject>();
        CollisionSystem collision = new CollisionSystemPersistentSAP();
        World = new World(collision)
        {
            AllowDeactivation = true,
        };
    }
    
    public static void UpdatePhysics(GameTime gameTime)
    {
        // World.Step((float)gameTime.ElapsedGameTime.TotalSeconds, true);
        var step = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if(step > 1.0f / 100.0f) step = 1.0f / 100.0f;
        World.Step(step,IsMultithread);
    }

    public static void RegisterPhysicsObject(PhysicsObject physicsObject)
    {
        if (_physicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to register the same physics object twice!", physicsObject);
            return;
        }
        physicsObject.PhysicsWorld = World;
        World.AddBody(physicsObject.RigidBody);
        _physicsObjects.Add(physicsObject);
    }
    
    public static void UnRegisterPhysicsObject(PhysicsObject physicsObject)
    {
        if (!_physicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to unregister a physics object that is not registered!", physicsObject);
            return;
        }
        physicsObject.PhysicsWorld = null;
        World.RemoveBody(physicsObject.RigidBody);
        _physicsObjects.Remove(physicsObject);
    }
    
    /// <summary>
    /// Checks if physics object 1 and object 2 intersect with one another
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static bool IsIntersecting(PhysicsObject obj1, PhysicsObject obj2)
    {
        return false;
        // return obj1.CheckCollision(obj2.GetBoundingBox());
    }
}