using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game.UI;

public class Canvas : IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;
    
    public void Draw(GameTime gameTime)
    {
        
    }
}