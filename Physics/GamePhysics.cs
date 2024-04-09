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

    public readonly Vector3 WorldGravity = worldGravity ?? new Vector3(0, -20f, 0);

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
    /// <param name="ignoredPhysicsObjects"></param>
    /// <returns></returns>
    public bool RaycastHitCheck(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance,
        ICollection<PhysicsObject> ignoredPhysicsObjects = null)
    {
        var intersectionPoint = Vector3.Zero;
        var hitNormal = Vector3.Zero;

        return PhysicsObjects
            .Where(physicsObject => ignoredPhysicsObjects == null || !ignoredPhysicsObjects.Contains(physicsObject))
            .Any(physicsObject => RayIntersects(rayOrigin, rayDirection, physicsObject.CollisionBox, maxDistance,
                ref intersectionPoint, ref hitNormal));
    }

    public bool Raycast(Vector3 rayOrigin, Vector3 rayDirection, out RaycastHit hit, float maxDistance,
        ICollection<PhysicsObject> ignoredPhysicsObjects = null)
    {
        hit = new RaycastHit();
        var closestDistance = float.MaxValue; // Initialize with a large value
        var intersectionPoint = Vector3.Zero;
        var hitNormal = Vector3.Zero;

        foreach (var physicsObject in PhysicsObjects)
        {
            // ignore if this physics object was marked to be ignored
            if (ignoredPhysicsObjects != null && ignoredPhysicsObjects.Contains(physicsObject))
                continue;

            if (!RayIntersects(rayOrigin, rayDirection, physicsObject.CollisionBox, maxDistance, ref intersectionPoint,
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

        // update the state of every physics object before checking for collisions
        foreach (var physicsObject in PhysicsObjects)
        {
            // Apply velocity to position
            if (!physicsObject.IsStatic)
            {
                physicsObject.Transform.Position += physicsObject.Velocity * deltaTime;
            }

            physicsObject.PhysicsTick(deltaTime);
        }

        // check the collision for every dynamic object
        foreach (var dynamicObject in PhysicsObjects.Where(x => !x.IsStatic))
        {
            foreach (var otherPhysicsObject in PhysicsObjects)
            {
                // ignore if this is our physics objects
                if (otherPhysicsObject == dynamicObject) continue;

                // check if a collision occurred with this other physics object
                if (dynamicObject.CollisionBox.Intersects(otherPhysicsObject.CollisionBox))
                {
                    HandleCollision(dynamicObject, otherPhysicsObject);
                }
            }
        }
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

    private void HandleCollision(PhysicsObject self, PhysicsObject other)
    {
        var penetrationDirection = Vector3.Zero;
        var penetration = CalculatePenetrationDepth(self.CollisionBox, other.CollisionBox, ref penetrationDirection);
        self.Transform.Position += penetrationDirection * penetration;

        var relativeVelocity = other.Velocity - self.Velocity;

        // Use the smaller of the two restitutions
        var restitution = MathF.Min(self.Restitution, other.Restitution);
        var impulseMagnitude = (1 + restitution) * Vector3.Dot(relativeVelocity, penetrationDirection) /
                               (1 / self.Mass + 1 / other.Mass);

        var impulse = impulseMagnitude * penetrationDirection;

        self.Velocity += impulse / self.Mass;
        if (!other.IsStatic)
            other.Velocity -= impulse / other.Mass;
    }

    private float CalculatePenetrationDepth(BoundingBox b1, BoundingBox b2, ref Vector3 penetrationNormal)
    {
        // Calculate overlap along each axis
        var overlapX = Math.Max(0, Math.Min(b1.Max.X, b2.Max.X) - Math.Max(b1.Min.X, b2.Min.X));
        var overlapY = Math.Max(0, Math.Min(b1.Max.Y, b2.Max.Y) - Math.Max(b1.Min.Y, b2.Min.Y));
        var overlapZ = Math.Max(0, Math.Min(b1.Max.Z, b2.Max.Z) - Math.Max(b1.Min.Z, b2.Min.Z));

        // Find the minimum overlap and corresponding axis
        var minOverlap = Math.Min(Math.Min(overlapX, overlapY), overlapZ);

        var b1Center = b1.Min + (b1.Max - b1.Min) / 2;
        var b2Center = b2.Min + (b2.Max - b2.Min) / 2;

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