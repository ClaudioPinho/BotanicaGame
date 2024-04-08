using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using TestMonoGame.Debug;
using TestMonoGame.Game;
using Vector3 = System.Numerics.Vector3;

namespace TestMonoGame.Physics;

// https://jitterphysics.com/docs/quickstart/hello-world
public class GamePhysics
{
    private struct NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b,
            ref float speculativeMargin)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold,
            out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 1f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB,
            ref ConvexContactManifold manifold)
        {
            return true;
        }

        public void Dispose()
        {
        }
    }

    private struct PoseIntegratorCallbacks(Vector3 gravity) : IPoseIntegratorCallbacks
    {
        public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public readonly bool AllowSubstepsForUnconstrainedBodies => false;
        public readonly bool IntegrateVelocityForKinematics => false;

        public Vector3 Gravity = gravity;

        Vector3Wide gravityWideDt;

        public void Initialize(Simulation simulation)
        {
        }

        public void PrepareForIntegration(float dt)
        {
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
            BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            velocity.Linear += gravityWideDt;
        }
    }

    public Simulation Simulation { get; private set; }
    
    private BufferPool _bufferPool;
    private ThreadDispatcher _threadDispatcher;

    // private float _timeAccumulator;

    public GamePhysics()
    {
        _bufferPool = new BufferPool();

        var targetThreadCount = int.Max(1,
            Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new ThreadDispatcher(targetThreadCount);

        // _timeAccumulator = 0;

        Simulation = BepuPhysics.Simulation.Create(_bufferPool, new NarrowPhaseCallbacks(),
            new PoseIntegratorCallbacks(new Vector3(0, -10, 0)), new SolveDescription(8, 1));
    }
    
    public void UpdatePhysics(GameTime gameTime)
    {
        // _timeAccumulator = (float)gameTime.TotalGameTime.TotalSeconds;
        //
        // var targetTimestepDuration = 1 / 120f;
        // while (_timeAccumulator >= targetTimestepDuration)
        // {
        //     Simulation.Timestep(targetTimestepDuration, ThreadDispatcher);
        //     _timeAccumulator -= targetTimestepDuration;
        // }

        // todo: i have to check if keeping the simulation locked makes sense for the game
        Simulation.Timestep(1 / 60f, _threadDispatcher);
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