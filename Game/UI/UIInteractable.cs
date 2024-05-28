using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Game.UI;

public class UIInteractable(string id) : UIGraphic(id), IHoverListener
{
    /// <summary>
    /// The area that should be interactable with this element
    /// </summary>
    public Rectangle InteractionArea
    {
        get => UseCustomInteractionArea ? _interactionArea ?? Rectangle.Empty : RealLocation;
        set => _interactionArea = value;
    }

    public bool UseCustomInteractionArea;

    public bool CanBeInteracted;

    public bool IsBeingHovered { get; protected set; }
    public bool IsSelected { get; protected set; }

    private Rectangle? _interactionArea;

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (Canvas == null) return;
        _interactionArea ??= GetInteractionArea();
        if (Canvas.DrawDebugBoxes && IsBeingHovered)
        {
            Canvas.SpriteBatch.Draw(MainGame.SquareOutlineTexture, new Rectangle(Position, Size), Color.Red);
        }
    }

    protected virtual Rectangle GetInteractionArea()
    {
        return new Rectangle(RealPosition, RealSize);
    }

    public override void OnCanvasScaleChanged()
    {
        base.OnCanvasScaleChanged();
        InteractionArea = GetInteractionArea();
    }

    public virtual void OnHoverStart()
    {
        IsBeingHovered = true;
    }

    public virtual void OnHoverEnd()
    {
        IsBeingHovered = false;
        IsSelected = false;
    }

    public virtual void OnSelected(MouseState mouseState)
    {
        IsSelected = true;
    }

    public virtual void OnDeselected(MouseState mouseState)
    {
        IsSelected = false;
    }
}