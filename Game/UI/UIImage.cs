using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIImage(Canvas canvas) : UIGraphics(canvas)
{
    public Texture2D Image
    {
        get => _image;
        set
        {
            _image = value;
            RecalculatePivotOrigin();
        }
    }

    private Texture2D _image;

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        spriteBatch.Draw(Image, DrawRectangle, Source, Color, Rotation, Origin, SpriteEffects.None, 0);
    }

    protected override Vector2 GetPivotOrigin()
    {
        return _image == null ? base.GetPivotOrigin() : new Vector2(_image.Width * Pivot.X, _image.Height * Pivot.Y);
    }
}