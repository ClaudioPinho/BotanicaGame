using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Game;

namespace TestMonoGame.Physics;

public class GamePhysics(Vector3? worldGravity = null)
{
    public readonly List<PhysicsObject> PhysicsObjects = [];

    private readonly Vector3 _worldGravity = worldGravity ?? new Vector3(0, -10f, 0);

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

    public void UpdatePhysics(float deltaTime)
    {
        // make sure each physics object updates
        PhysicsObjects.ForEach(x => x.EarlyPhysicsTick(deltaTime));

        var dynamicPhysicsObjects = PhysicsObjects.Where(x => !x.IsStatic).ToList();

        // check if a collision is detected
        foreach (var dynamicPhysicsObject in dynamicPhysicsObjects)
        {
            var futureBoundingBox = dynamicPhysicsObject.PredictNextBoundingBox(deltaTime);
            
            // add gravity to the velocity
            dynamicPhysicsObject.Velocity += _worldGravity * deltaTime;

            foreach (var otherPhysicsObject in PhysicsObjects)
            {
                if (otherPhysicsObject == dynamicPhysicsObject ||
                    !futureBoundingBox.Intersects(otherPhysicsObject.CollisionBox))
                    continue;

                // Perform swept AABB collision detection
                var collisionTime = SweptAABBCollisionTime(dynamicPhysicsObject.CollisionBox, futureBoundingBox,
                    otherPhysicsObject.CollisionBox, deltaTime);
                
                DebugUtils.PrintMessage(collisionTime.ToString());

                if (collisionTime is >= 0 and <= 1)
                {
                    // Handle collision between dynamicPhysicsObject and otherPhysicsObject
                    // Adjust positions and velocities accordingly
                    // You may also need to handle multiple collisions and resolve them properly
                    HandleCollision(otherPhysicsObject, otherPhysicsObject, collisionTime);
                }
            }

            DebugUtils.DrawWireCube(dynamicPhysicsObject.Transform.Position,
                customCorners: dynamicPhysicsObject.CollisionBox.GetCorners());
        }

        PhysicsObjects.ForEach(x => x.LatePhysicsTick(deltaTime));

        foreach (var dynamicObject in dynamicPhysicsObjects)
        {
            dynamicObject.Transform.Position += dynamicObject.Velocity * deltaTime;
        }
    }
    
    public void HandleCollision(PhysicsObject object1, PhysicsObject object2, float collisionTime)
    {
        // Calculate the new position of object1 at the time of collision
        var newPosition1 = object1.Transform.Position + object1.Velocity * collisionTime;

        // Adjust the position of object1 to prevent overlap
        var penetrationDepth = GetPenetrationDepth(object1.CollisionBox, object2.CollisionBox);
        object1.Transform.Position += penetrationDepth;

        // If object2 is static (doesn't move), stop the movement of object1
        if (object2.IsStatic)
        {
            object1.Velocity = Vector3.Zero;
        }
        else
        {
            // Reflect velocities based on the collision normal (for elastic collisions)
            var collisionNormal = Vector3.Normalize(object2.Transform.Position - object1.Transform.Position);
            var relativeVelocity = object2.Velocity - object1.Velocity;
            var dotProduct = Vector3.Dot(relativeVelocity, collisionNormal);

            object1.Velocity += dotProduct * collisionNormal;
        }

        // Optionally, apply friction or other constraints to the velocities to simulate different materials or conditions
    }

    public float SweptAABBCollisionTime(BoundingBox movingBox1, BoundingBox futureBox1, BoundingBox staticBox2,
        float deltaTime)
    {
        // Calculate relative velocity
        var relativeVelocity = (futureBox1.Min - movingBox1.Min) / deltaTime;

        // Calculate the time of first and last collision along each axis
        var tFirst = new Vector3();
        var tLast = new Vector3();

        for (var i = 0; i < 3; i++)
        {
            if (relativeVelocity.X < 0)
            {
                tFirst.X = (staticBox2.Max.X - movingBox1.Min.X) / relativeVelocity.X;
                tLast.X = (staticBox2.Min.X - movingBox1.Max.X) / relativeVelocity.X;
            }
            else if (relativeVelocity.X > 0)
            {
                tFirst.X = (staticBox2.Min.X - movingBox1.Max.X) / relativeVelocity.X;
                tLast.X = (staticBox2.Max.X - movingBox1.Min.X) / relativeVelocity.X;
            }
            else
            {
                // If relative velocity is zero along an axis, set tFirst and tLast to positive infinity
                // This ensures that the division doesn't produce NaN and prevents division by zero
                tFirst.X = float.PositiveInfinity;
                tLast.X = float.PositiveInfinity;
            }

            if (relativeVelocity.Y < 0)
            {
                tFirst.Y = (staticBox2.Max.Y - movingBox1.Min.Y) / relativeVelocity.Y;
                tLast.Y = (staticBox2.Min.Y - movingBox1.Max.Y) / relativeVelocity.Y;
            }
            else if (relativeVelocity.Y > 0)
            {
                tFirst.Y = (staticBox2.Min.Y - movingBox1.Max.Y) / relativeVelocity.Y;
                tLast.Y = (staticBox2.Max.Y - movingBox1.Min.Y) / relativeVelocity.Y;
            }
            else
            {
                // If relative velocity is zero along an axis, set tFirst and tLast to positive infinity
                // This ensures that the division doesn't produce NaN and prevents division by zero
                tFirst.Y = float.PositiveInfinity;
                tLast.Y = float.PositiveInfinity;
            }

            if (relativeVelocity.Z < 0)
            {
                tFirst.Z = (staticBox2.Max.Z - movingBox1.Min.Z) / relativeVelocity.Z;
                tLast.Z = (staticBox2.Min.Z - movingBox1.Max.Z) / relativeVelocity.Z;
            }
            else if (relativeVelocity.Z > 0)
            {
                tFirst.Z = (staticBox2.Min.Z - movingBox1.Max.Z) / relativeVelocity.Z;
                tLast.Z = (staticBox2.Max.Z - movingBox1.Min.Z) / relativeVelocity.Z;
            }
            else
            {
                // If relative velocity is zero along an axis, set tFirst and tLast to positive infinity
                // This ensures that the division doesn't produce NaN and prevents division by zero
                tFirst.Z = float.PositiveInfinity;
                tLast.Z = float.PositiveInfinity;
            }
        }

        // Find the earliest and latest times of collision
        var tEnter = Math.Max(Math.Max(tFirst.X, tFirst.Y), tFirst.Z);
        var tExit = Math.Min(Math.Min(tLast.X, tLast.Y), tLast.Z);

        // If the time of first collision along any axis is greater than the time of last collision along any axis, no collision occurs
        if (tEnter > tExit || tFirst.X < 0 && tFirst.Y < 0 && tFirst.Z < 0 || tEnter > 1 || tEnter < 0)
            return -1;

        return tEnter;
    }

    // private void ResolveCollision(PhysicsObject baseObject, PhysicsObject otherObject)
    // {
    //     var penetration = GetPenetrationDepth(baseObject.WorldBoundingBox, otherObject.WorldBoundingBox);
    //     
    //     var separation = Vector3.Zero;
    //     if (Math.Abs(penetration.X) < Math.Abs(penetration.Y))
    //         separation.X = penetration.X;
    //     else
    //         separation.Y = penetration.Y;
    //
    //     if (Math.Abs(penetration.Z) < Math.Abs(penetration.Y))
    //         separation.Z = penetration.Z;
    //     else
    //         separation.Y = penetration.Y;
    //
    //     baseObject.Transform.Position -= separation;
    //
    //     baseObject.Velocity *= -0.5f;
    //
    // }

    public static Vector3 GetPenetrationDepth(BoundingBox bb1, BoundingBox bb2)
    {
        // Calculate the distances between the AABBs along each axis
        var xOverlap = Math.Max(0, Math.Min(bb1.Max.X, bb2.Max.X) - Math.Max(bb1.Min.X, bb2.Min.X));
        var yOverlap = Math.Max(0, Math.Min(bb1.Max.Y, bb2.Max.Y) - Math.Max(bb1.Min.Y, bb2.Min.Y));
        var zOverlap = Math.Max(0, Math.Min(bb1.Max.Z, bb2.Max.Z) - Math.Max(bb1.Min.Z, bb2.Min.Z));

        // Return the penetration depth along each axis
        return new Vector3(xOverlap, yOverlap, zOverlap);
    }

    /// <summary>
    /// Checks if physics object 1 and object 2 intersect with one another
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public bool IsIntersecting(PhysicsObject obj1, PhysicsObject obj2)
    {
        return obj1.CollisionBox.Intersects(obj2.CollisionBox);
    }
}