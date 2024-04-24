using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Game.UI;

public class UIButton : UIImage
{
    public Texture2D HoverTexture;
    public Texture2D SelectedTexture;

    public Action OnButtonClicked;

    public UIButton(Canvas canvas) : base(canvas)
    {
        CanBeInteracted = true;
    }

    protected override Texture2D GetTextureToDraw()
    {
        if (IsSelected && SelectedTexture != null) return SelectedTexture;
        if (IsBeingHovered && HoverTexture != null) return HoverTexture;
        return Image;
    }

    protected override Rectangle GetInteractionArea()
    {
        if (IsBeingHovered && HoverTexture == null || !IsBeingHovered && Image == null)
            return base.GetInteractionArea();

        var textureForInteraction = IsBeingHovered ? HoverTexture : Image;
        return new Rectangle(Position, new Point(textureForInteraction.Width, textureForInteraction.Height));
    }

    public override void OnDeselected(MouseState mouseState)
    {
        base.OnDeselected(mouseState);
        if (mouseState.LeftButton == ButtonState.Released)
            OnButtonClicked?.Invoke();
    }
}