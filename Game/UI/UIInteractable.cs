using Microsoft.Xna.Framework;

namespace TestMonoGame.Game.UI;

public class UIInteractable : UIGraphics, IHoverListener
{
    public Rectangle InteractionArea;
    
    public bool IsBeingHovered { get; protected set; }
    
    public UIInteractable(Canvas canvas) : base(canvas)
    {
        InteractionArea = GetInteractionArea();
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
    }
}