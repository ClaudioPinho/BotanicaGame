using Jitter;
using Jitter.LinearMath;
using TestMonoGame.Extensions;

namespace TestMonoGame.Debug;

public class DebugDrawCollision : IDebugDrawer
{

    public void DrawLine(JVector start, JVector end)
    {
        DebugUtils.PrintMessage("should be drawing a line!");
    }

    public void DrawPoint(JVector pos)
    {
        DebugUtils.DrawWirePoint(pos.ToVector3());
    }

    public void DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
    {   
        DebugUtils.DrawTriangle(pos1.ToVector3(), pos2.ToVector3(), pos3.ToVector3());
    }
}