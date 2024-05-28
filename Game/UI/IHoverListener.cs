using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Game.UI;

public interface IHoverListener
{
    public void OnHoverStart();
    public void OnHoverEnd();
    public void OnSelected(MouseState mouseState);
    public void OnDeselected(MouseState mouseState);
}