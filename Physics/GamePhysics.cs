using System;
using System.Collections.Generic;
using Jitter2;
using Jitter2.Parallelization;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

// https://jitterphysics.com/docs/quickstart/hello-world
public static class GamePhysics
{
    public static World World { get; private set; }

    private static List<PhysicsObject> _physicsObjects;

    public static void Initialize()
    {
        ThreadPool.Instance.ChangeThreadCount(1);
        _physicsObjects = new List<PhysicsObject>();
        World = new World();
    }

    public static void UpdatePhysics(GameTime gameTime)
    {
        World.Step((float)gameTime.ElapsedGameTime.TotalSeconds, true);
    }

    public static void RegisterPhysicsObject(PhysicsObject physicsObject)
    {
        if (_physicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to register the same physics object twice!", physicsObject);
            return;
        }
        _physicsObjects.Add(physicsObject);
    }
    
    public static void UnRegisterPhysicsObject(PhysicsObject physicsObject)
    {
        if (!_physicsObjects.Contains(physicsObject))
        {
            DebugUtils.PrintWarning("Tried to unregister a physics object that is not registered!", physicsObject);
            return;
        }
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