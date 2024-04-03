using Jitter2;
using Jitter2.LinearMath;
using TestMonoGame.Extensions;

namespace TestMonoGame.Debug;

public class DebugDrawCollision : IDebugDrawer
{
    public void DrawSegment(in JVector pA, in JVector pB)
    {
        // DebugUtils.PrintMessage("drawing segment");
    }

    public void DrawTriangle(in JVector pA, in JVector pB, in JVector pC)
    {
        DebugUtils.DrawTriangle(pA.ToVector3(), pB.ToVector3(), pC.ToVector3());
    }

    public void DrawPoint(in JVector p)
    {
        DebugUtils.DrawWirePoint(p.ToVector3());
        DebugUtils.PrintMessage("drawing point");
    }
}