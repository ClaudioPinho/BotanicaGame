using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIImage(Texture2D texture) : UIGraphics
{
    public Texture2D Image = texture;

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        spriteBatch.Draw(Image, Destination, Source, Color, Rotation, Origin, SpriteEffects.None, 0);
    }
}