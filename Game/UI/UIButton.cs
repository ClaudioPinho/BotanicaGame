using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIButton : UIInteractable
{
    public Texture2D DefaultTexture;
    public Texture2D HoverTexture;


    public UIButton(Canvas canvas) : base(canvas)
    {
    }

    protected override Rectangle GetInteractionArea()
    {
        if (IsBeingHovered && HoverTexture == null || !IsBeingHovered && DefaultTexture == null)
            return base.GetInteractionArea();

        var textureForInteraction = IsBeingHovered ? HoverTexture : DefaultTexture;
        return new Rectangle(Position, new Point(textureForInteraction.Width, textureForInteraction.Height));
    }
}