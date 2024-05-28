using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Game.UI;

public class UIImage(string id) : UIInteractable(id)
{
    public Texture2D Image
    {
        get => _image ?? MainGame.SinglePixelTexture;
        set
        {
            _image = value;
            Size = new Point(value.Width, value.Height);
            RecalculatePivotOrigin();
        }
    }

    private Texture2D _image;

    protected virtual Texture2D GetTextureToDraw() => Image;

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (Canvas == null) return;
        Canvas.SpriteBatch.Draw(GetTextureToDraw(), DrawRectangle, Source, Color, Rotation, Origin, SpriteEffects.None, 0);
    }

    protected override Vector2 GetPivotOrigin()
    {
        return _image == null ? base.GetPivotOrigin() : new Vector2(_image.Width * Pivot.X, _image.Height * Pivot.Y);
    }
}