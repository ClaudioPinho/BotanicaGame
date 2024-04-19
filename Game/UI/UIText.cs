using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIText : UIGraphics
{
    public string Text;
    public SpriteFont Font;

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        spriteBatch.DrawString(Font, Text, Position, Color);
    }
}