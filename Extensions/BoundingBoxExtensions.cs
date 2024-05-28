using Microsoft.Xna.Framework;

namespace BotanicaGame.Extensions;

public static class BoundingBoxExtensions
{
    public static BoundingBox Transform(this BoundingBox box, Matrix matrix)
    {
        var corners = box.GetCorners();
        Vector3.Transform(corners, ref matrix, corners);

        // Recalculate the bounding box using the transformed corners
        return BoundingBox.CreateFromPoints(corners);
    }
}