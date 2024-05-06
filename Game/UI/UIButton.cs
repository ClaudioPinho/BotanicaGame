using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Game.UI;

public class UIButton : UIImage
{
    public Texture2D HoverTexture;
    public Texture2D SelectedTexture;

    public Action OnButtonClicked;

    public SoundEffect OnClickInSound;
    public SoundEffect OnClickOutSound;

    public UIButton(Canvas canvas, SpriteBatch spriteBatch) : base(canvas, spriteBatch)
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

        var textureForInteraction = GetTextureToDraw();
        return new Rectangle(RealPosition,
            (new Point(textureForInteraction.Width, textureForInteraction.Height).ToVector2() * Canvas.CanvasScale)
            .ToPoint());
    }

    protected virtual void OnButtonWasClickedIn()
    {
        GameAudio.PlaySoundEffect(OnClickInSound, GameAudio.EAudioGroup.UI);
    }

    protected virtual void OnButtonWasClickedOut()
    {
        OnButtonClicked?.Invoke();
        GameAudio.PlaySoundEffect(OnClickOutSound, GameAudio.EAudioGroup.UI);
    }

    public override void OnSelected(MouseState mouseState)
    {
        base.OnSelected(mouseState);
        if (mouseState.LeftButton == ButtonState.Pressed)
            OnButtonWasClickedIn();
    }

    public override void OnDeselected(MouseState mouseState)
    {
        base.OnDeselected(mouseState);
        if (mouseState.LeftButton == ButtonState.Released)
            OnButtonWasClickedOut();
    }
}