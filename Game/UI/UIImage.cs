using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIImage(string name) : UIGraphics(name)
{
    public Texture2D Image;

    // public override void Draw(SpriteBatch spriteBatch, float deltaTime)
    // {
    //     base.Draw(spriteBatch, deltaTime);
    //     spriteBatch.Draw(Image, Destination, Source, Color, Rotation, Origin, SpriteEffects.None, DepthLayer);
    // }
}