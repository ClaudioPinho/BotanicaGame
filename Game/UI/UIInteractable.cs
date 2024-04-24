using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Game.UI;

public class UIInteractable : UIGraphics, IHoverListener
{
    /// <summary>
    /// The area that should be interactable with this element
    /// </summary>
    public Rectangle InteractionArea
    {
        get => UseCustomInteractionArea ? _interactionArea : RealLocation;
        set => _interactionArea = value;
    }

    public bool UseCustomInteractionArea;

    public bool CanBeInteracted;

    public bool IsBeingHovered { get; protected set; }
    public bool IsSelected { get; protected set; }

    private Rectangle _interactionArea;

    public UIInteractable(Canvas canvas) : base(canvas)
    {
        InteractionArea = GetInteractionArea();
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        base.Draw(spriteBatch, gameTime);
        if (Canvas.DrawDebugBoxes && IsBeingHovered)
        {
            spriteBatch.Draw(MainGame.SquareOutlineTexture, new Rectangle(Position, Size), Color.Red);
        }
    }

    protected virtual Rectangle GetInteractionArea()
    {
        return new Rectangle(Position, Size);
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