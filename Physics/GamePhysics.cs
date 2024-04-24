using System;
using System.Collections.Generic;
using System.Linq;
using BotanicaGame.Debug;
using BotanicaGame.Game;
using Microsoft.Xna.Framework;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace BotanicaGame.Physics;

// https://gamedev.net/tutorials/programming/general-and-gameplay-programming/swept-aabb-collision-detection-and-response-r3084/
public class GamePhysics(Vector3? worldGravity = null)
{
    public const int MaxCollisionDistance = 2;

    public readonly List<PhysicsObject> PhysicsObjects = [];

    public readonly Vector3 WorldGravity = worldGravity ?? new Vector3(0, -30f, 0);

    private Vector3 _sweptNormal;
    private Vector3 _penetrationNormal;
    private float _penetrationDepth;
    private float _collisionTime;
    private float _remainingTime;
    private bool _anyCollisionDetected;

    private Vector3 _intersectionPoint;
    private Vector3 _hitNormal;

    private BoundingBox _boxCastSource;
    private Vector3 _boxDestinyPosition;

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

    /// <summary>
    /// Simple raycast that just checks if any hit occurred at all
    /// </summary>
    /// <param name="rayOrigin"></param>
    /// <param name="rayDirection"></param>
    /// <param name="maxDistance"></param>
    /// <param name="ignoredPhysicsObject"></param>
    /// <returns></returns>
    public bool RaycastHitCheck(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance,
        PhysicsObject ignoredPhysicsObject = null)
    {
        _intersectionPoint = Vector3.Zero;
        _hitNormal = Vector3.Zero;

        return PhysicsObjects
            .Where(physicsObject => ignoredPhysicsObject == null || ignoredPhysicsObject != physicsObject)
            .Any(physicsObject => RayIntersects(rayOrigin, rayDirection, physicsObject.CollisionBox, maxDistance,
                ref _intersectionPoint, ref _hitNormal));
    }

    public bool BoxcastHitCheck(Vector3 boxOrigin, Vector3 boxDirection, Vector3 boxSize, float maxDistance,
        PhysicsObject ignoredPhysicsObject)
    {
        // Update _boxCastSource with the new origin
        _boxCastSource.Min = boxOrigin - boxSize * 0.5f;
        _boxCastSource.Max = boxOrigin + boxSize * 0.5f;

        // Calculate the destiny position
        _boxDestinyPosition = boxOrigin + boxDirection * maxDistance;

        _boxCastSource.Min = Vector3.Min(_boxCastSource.Min, _boxDestinyPosition - boxSize * 0.5f);
        _boxCastSource.Max = Vector3.Max(_boxCastSource.Max, _boxDestinyPosition + boxSize * 0.5f);

        // DebugUtils.DrawWireCube(boxOrigin, customCorners: _boxCastSource.GetCorners(), color: Color.Red);

        // Check for intersection with static objects
        return PhysicsObjects
            .Where(staticObject => staticObject.IsStatic && staticObject != ignoredPhysicsObject)
            .Any(staticObject => _boxCastSource.Intersects(staticObject.CollisionBox));
    }

    public bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, ref RaycastHit hit, float maxDistance,
        PhysicsObject ignoredPhysicsObject = null)
    {
        var closestDistance = float.MaxValue; // Initialize with a large value
        var intersectionPoint = Vector3.Zero;
        var hitNormal = Vector3.Zero;

        foreach (var physicsObject in PhysicsObjects)
        {
            // ignore if this physics object was marked to be ignored
            if (physicsObject == ignoredPhysicsObject)
                continue;

            if (!RayIntersects(rayOrigin, rayDirection, physicsObject.CollisionBox, maxDistance,
                ref intersectionPoint,
                ref hitNormal)) continue;

            // Calculate the distance from the ray origin to the intersection point
            var distance = Vector3.Distance(rayOrigin, intersectionPoint);

            // Check if this intersection point is closer than the current closest one
            if (!(distance < closestDistance)) continue;
            // Update the closest intersection point and other hit information
            closestDistance = distance;
            hit.HitPosition = intersectionPoint;
            hit.GameObjectHit = physicsObject;
            hit.HitNormal = hitNormal;
        }

        // Check if any intersection was found
        return closestDistance < float.MaxValue;
    }

    public void UpdatePhysics(float deltaTime)
    {
        ApplyGravity(deltaTime);

        foreach (var physicsObject in PhysicsObjects)
        {
            physicsObject.CalculateAABB();
            physicsObject.PhysicsTick(deltaTime);
        }

        // check collisions only for dynamic objects
        foreach (var physicsObject in PhysicsObjects.Where(x => !x.IsStatic))
        {
            _anyCollisionDetected = false;

            foreach (var otherPhysicsObject in PhysicsObjects)
            {
                // ignore self
                if (otherPhysicsObject == physicsObject) continue;

                // do a simple broad-phase collision check first
                if (!GetSweptBroadphaseBox(physicsObject, deltaTime)
                    .Intersects(otherPhysicsObject.CollisionBox)) continue;

                _collisionTime = SweptAABB(physicsObject.CollisionBox, otherPhysicsObject.CollisionBox,
                    physicsObject.Velocity * deltaTime, ref _sweptNormal);
                _remainingTime = 1.0f - _collisionTime;

                _penetrationDepth = CalculatePenetrationDepth(
                    PredictAABBAtNextFrame(physicsObject, physicsObject.Velocity, deltaTime),
                    otherPhysicsObject.CollisionBox,
                    ref _penetrationNormal);

                // ignore any collision that doesn't really penetrate enough into the other collider
                if (_collisionTime is >= 0.0f and <= 1.0f /*&& _penetrationDepth > 0.0f*/)
                {
                    HandleSweptCollision(physicsObject, otherPhysicsObject, deltaTime);
                }
                else if (_penetrationDepth > 0)
                {
                    HandleCollision(physicsObject, otherPhysicsObject, deltaTime);
                }
            }

            if (!_anyCollisionDetected)
            {
                physicsObject.Transform.Position += physicsObject.Velocity * deltaTime;
            }
        }
    }

    private float SweptAABB(BoundingBox self, BoundingBox other, Vector3 selfVelocity, ref Vector3 normal)
    {
        normal = Vector3.Zero;

        var distanceEntry = Vector3.Zero;
        var distanceExit = Vector3.Zero;

        if (selfVelocity.X > 0.0f)
        {
            distanceEntry.X = other.Min.X - self.Max.X;
            distanceExit.X = other.Max.X - self.Min.X;
        }
        else
        {
            distanceEntry.X = other.Max.X - self.Min.X;
            distanceExit.X = other.Min.X - self.Max.X;
        }

        if (selfVelocity.Y > 0.0f)
        {
            distanceEntry.Y = other.Min.Y - self.Max.Y;
            distanceExit.Y = other.Max.Y - self.Min.Y;
        }
        else
        {
            distanceEntry.Y = other.Max.Y - self.Min.Y;
            distanceExit.Y = other.Min.Y - self.Max.Y;
        }

        if (selfVelocity.Z > 0.0f)
        {
            distanceEntry.Z = other.Min.Z - self.Max.Z;
            distanceExit.Z = other.Max.Z - self.Min.Z;
        }
        else
        {
            distanceEntry.Z = other.Max.Z - self.Min.Z;
            distanceExit.Z = other.Min.Z - self.Max.Z;
        }

        var entryTimeVector = Vector3.Zero;
        var exitTimeVector = Vector3.Zero;

        if (selfVelocity.X == 0.0f)
        {
            if (MathF.Max(MathF.Abs(distanceEntry.X), MathF.Abs(distanceExit.X)) > (self.Max.X - self.Min.X) +
                (other.Max.X - other.Min.X))
            {
                entryTimeVector.X = 2.0f;
            }
            else
            {
                entryTimeVector.X = float.NegativeInfinity;
            }

            exitTimeVector.X = float.PositiveInfinity;
        }
        else
        {
            entryTimeVector.X = distanceEntry.X / selfVelocity.X;
            exitTimeVector.X = distanceExit.X / selfVelocity.X;
        }

        if (selfVelocity.Y == 0.0f)
        {
            if (MathF.Max(MathF.Abs(distanceEntry.Y), MathF.Abs(distanceExit.Y)) > (self.Max.Y - self.Min.Y) +
                (other.Max.Y - other.Min.Y))
            {
                entryTimeVector.Y = 2.0f;
            }
            else
            {
                entryTimeVector.Y = float.NegativeInfinity;
            }

            exitTimeVector.Y = float.PositiveInfinity;
        }
        else
        {
            entryTimeVector.Y = distanceEntry.Y / selfVelocity.Y;
            exitTimeVector.Y = distanceExit.Y / selfVelocity.Y;
        }

        if (selfVelocity.Z == 0.0f)
        {
            if (MathF.Max(MathF.Abs(distanceEntry.Z), MathF.Abs(distanceExit.Z)) > (self.Max.Z - self.Min.Z) +
                (other.Max.Z - other.Min.Z))
            {
                entryTimeVector.Z = 2.0f;
            }
            else
            {
                entryTimeVector.Z = float.NegativeInfinity;
            }

            exitTimeVector.Z = float.PositiveInfinity;
        }
        else
        {
            entryTimeVector.Z = distanceEntry.Z / selfVelocity.Z;
            exitTimeVector.Z = distanceExit.Z / selfVelocity.Z;
        }

        var entryTime = MathF.Max(MathF.Max(entryTimeVector.X, entryTimeVector.Y), entryTimeVector.Z);
        var exitTime = MathF.Min(MathF.Min(exitTimeVector.X, exitTimeVector.Y), exitTimeVector.Z);

        if (entryTime > exitTime || entryTimeVector is { X: < 0.0f, Y: < 0.0f, Z: < 0.0f } ||
            entryTimeVector.X > 1.0f || entryTimeVector.Y > 1.0f || entryTimeVector.Z > 1.0f)
        {
            return 2.0f;
        }

        if (entryTimeVector.X > entryTimeVector.Y && entryTimeVector.X > entryTimeVector.Z)
        {
            if (selfVelocity.X > 0.0f)
            {
                normal.X = -1.0f;
            }
            else
            {
                normal.X = 1.0f;
            }
            // if (distanceEntry.X > 0.0f)
            // {
            //     normal.X = 1.0f;
            // }
            // else
            // {
            //     normal.X = -1.0f;
            // }
        }
        else if (entryTimeVector.Y > entryTimeVector.X && entryTimeVector.Y > entryTimeVector.Z)
        {
            if (selfVelocity.Y > 0.0f)
            {
                normal.Y = -1.0f;
            }
            else
            {
                normal.Y = 1.0f;
            }
            // if (distanceEntry.Y < 0.0f)
            // {
            //     normal.Y = 1.0f;
            // }
            // else
            // {
            //     normal.Y = -1.0f;
            // }
        }
        else if (entryTimeVector.Z > entryTimeVector.X && entryTimeVector.Z > entryTimeVector.Y)
        {
            if (selfVelocity.Z > 0.0f)
            {
                normal.Z = -1.0f;
            }
            else
            {
                normal.Z = 1.0f;
            }
            // if (distanceEntry.Z < 0.0f)
            // {
            //     normal.Z = 1.0f;
            // }
            // else
            // {
            //     normal.Z = -1.0f;
            // }
        }

        return entryTime;
    }

    private BoundingBox PredictAABBAtNextFrame(PhysicsObject self, Vector3 velocity, float deltaTime)
    {
        return self.GetCollisionBoxAtPosition(self.Transform.Position + velocity * deltaTime);
    }

    private BoundingBox GetSweptBroadphaseBox(PhysicsObject obj, float deltaTime)
    {
        // Combine current and next AABB to form swept AABB
        return BoundingBox.CreateMerged(obj.CollisionBox,
            obj.GetCollisionBoxAtPosition(obj.Transform.Position + obj.Velocity * deltaTime));
    }

    private float CalculatePenetrationDepth(BoundingBox b1, BoundingBox b2, ref Vector3 penetrationNormal)
    {
        // Calculate overlap along each axis
        var overlapX = Math.Max(0, Math.Min(b1.Max.X, b2.Max.X) - Math.Max(b1.Min.X, b2.Min.X));
        var overlapY = Math.Max(0, Math.Min(b1.Max.Y, b2.Max.Y) - Math.Max(b1.Min.Y, b2.Min.Y));
        var overlapZ = Math.Max(0, Math.Min(b1.Max.Z, b2.Max.Z) - Math.Max(b1.Min.Z, b2.Min.Z));

        // Find the minimum overlap and corresponding axis
        var minOverlap = Math.Min(Math.Min(overlapX, overlapY), overlapZ);

        var b1Center = b1.Min + (b1.Max - b1.Min) * 0.5f;
        var b2Center = b2.Min + (b2.Max - b2.Min) * 0.5f;

        // Determine the penetration direction based on the axis of minimum overlap
        if (minOverlap == overlapX)
        {
            penetrationNormal = b1Center.X < b2Center.X ? Vector3.Left : Vector3.Right;
        }
        else if (minOverlap == overlapY)
        {
            penetrationNormal = b1Center.Y < b2Center.Y ? Vector3.Down : Vector3.Up;
        }
        else // overlap along Z axis
        {
            penetrationNormal = b1Center.Z < b2Center.Z ? Vector3.Forward : Vector3.Backward;
        }

        return minOverlap;
    }

    private void ApplyGravity(float deltaTime)
    {
        foreach (var physicsObject in PhysicsObjects.Where(x => !x.IsStatic && x.IsAffectedByGravity))
        {
            // Apply gravitational force (F = m * g)
            var gravitationalForce = WorldGravity * physicsObject.Mass;

            // Apply force to object (F = m * a => a = F / m)
            var accelerationDueToGravity = gravitationalForce / physicsObject.Mass;

            // Update object's velocity (v = u + at => u = 0)
            physicsObject.Velocity += accelerationDueToGravity * deltaTime;
        }
    }

    private void HandleCollision(PhysicsObject physicsObject, PhysicsObject other, float deltaTime)
    {
        if (!other.IsStatic && other.CanBePushed)
        {
            other.Transform.Position += physicsObject.Velocity / 2 * deltaTime;
            other.CalculateAABB();
        }
        
        // _anyCollisionDetected = true;

        // DebugUtils.DrawWireCube(other.Transform.Position,
        //     customCorners: other.CollisionBox.GetCorners());

        var velocityToResolve = Vector3.Zero;

        // if (physicsObject.Velocity.X > 0f)
        // {
        //     DebugUtils.PrintMessage("t");
        // }

        if (MathF.Abs(_penetrationNormal.X) > 0.0001f)
        {
            velocityToResolve.X = physicsObject.Velocity.X;
        }

        if (MathF.Abs(_penetrationNormal.Y) > 0.0001f)
        {
            velocityToResolve.Y = physicsObject.Velocity.Y;
        }

        if (MathF.Abs(_penetrationNormal.Z) > 0.0001f)
        {
            velocityToResolve.Z = physicsObject.Velocity.Z;
        }

        // move the player along the axis that we want to resolve
        physicsObject.Transform.Position += velocityToResolve * deltaTime;

        // cancel out the velocity on the resolved axis

        if (MathF.Abs(_penetrationNormal.X) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            physicsObject.Velocity.X = 0;
            // physicsObject.Velocity.X *= -0.9f;
        }

        if (MathF.Abs(_penetrationNormal.Y) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            // if (physicsObject.Velocity.Y < 0.01f)
            // physicsObject.Velocity.Y *= -0.9f;
            physicsObject.Velocity.Y = 0;
        }

        if (MathF.Abs(_penetrationNormal.Z) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            physicsObject.Velocity.Z = 0;
            // physicsObject.Velocity.Z *= -0.9f;
        }

        // resolve the collision by moving along the normal and the penetration depth
        physicsObject.Transform.Position += _penetrationNormal * _penetrationDepth;

        // recalculate the object's AABB for the next check
        physicsObject.CalculateAABB();
    }

    private void HandleSweptCollision(PhysicsObject physicsObject, PhysicsObject otherObject, float deltaTime)
    {
        _anyCollisionDetected = true;

        var velocity = physicsObject.Velocity;

        if (MathF.Abs(_sweptNormal.X) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            // velocity.X *= -.2f;
            velocity.X = 0;
        }

        if (MathF.Abs(_sweptNormal.Y) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            // velocity.Y *= -.2f;
            velocity.Y = 0;
        }

        if (MathF.Abs(_sweptNormal.Z) > 0.0001f)
        {
            // Bounce the velocity along that axis.
            // velocity.Z *= -.2f;
            velocity.Z = 0;
        }

        physicsObject.Transform.Position += physicsObject.Velocity * (_collisionTime * deltaTime);

        physicsObject.Velocity = velocity;

        physicsObject.Transform.Position += physicsObject.Velocity * (_remainingTime * deltaTime);

        physicsObject.CalculateAABB();
    }

    private bool RayIntersects(Vector3 rayOrigin, Vector3 rayDirection, BoundingBox boundingBox, float maxDistance,
        ref Vector3 intersectionPoint, ref Vector3 hitNormal)
    {
        // Calculate ray direction reciprocal to avoid division inside the loop
        var directionReciprocal = new Vector3(1.0f / rayDirection.X, 1.0f / rayDirection.Y, 1.0f / rayDirection.Z);

        // Calculate slab intersection distances
        var t1 = (boundingBox.Min.X - rayOrigin.X) * directionReciprocal.X;
        var t2 = (boundingBox.Max.X - rayOrigin.X) * directionReciprocal.X;
        var t3 = (boundingBox.Min.Y - rayOrigin.Y) * directionReciprocal.Y;
        var t4 = (boundingBox.Max.Y - rayOrigin.Y) * directionReciprocal.Y;
        var t5 = (boundingBox.Min.Z - rayOrigin.Z) * directionReciprocal.Z;
        var t6 = (boundingBox.Max.Z - rayOrigin.Z) * directionReciprocal.Z;

        // Find entry and exit distances for the ray
        var tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
        var tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

        // Check if the ray intersects the AABB
        if (tmax < 0 || tmin > tmax || tmin > maxDistance)
        {
            // No intersection or intersection behind the ray origin or beyond max distance
            return false;
        }

        // Calculate intersection point
        intersectionPoint = rayOrigin + rayDirection * tmin;

        // Determine hit normal based on the axis with the smallest overlap
        if (tmin == t1)
        {
            hitNormal = -Vector3.UnitX; // Hit from the left (negative X direction)
        }
        else if (tmin == t2)
        {
            hitNormal = Vector3.UnitX; // Hit from the right (positive X direction)
        }
        else if (tmin == t3)
        {
            hitNormal = -Vector3.UnitY; // Hit from below (negative Y direction)
        }
        else if (tmin == t4)
        {
            hitNormal = Vector3.UnitY; // Hit from above (positive Y direction)
        }
        else if (tmin == t5)
        {
            hitNormal = -Vector3.UnitZ; // Hit from behind (negative Z direction)
        }
        else if (tmin == t6)
        {
            hitNormal = Vector3.UnitZ; // Hit from in front (positive Z direction)
        }

        return true;
    }
}