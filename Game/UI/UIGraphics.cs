using System;
using Microsoft.Xna.Framework;

namespace TestMonoGame.Game.UI;

public class UIGraphics(string name) : GameObject(name), IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;
    
    public Rectangle Destination = new(0,0,100,100);
    public Rectangle? Source = null;
    
    public Vector2 Origin = Vector2.Zero;

    public Vector2 Scale = Vector2.One;
    public Color Color = Color.White;

    public float Rotation = 0f;

    public virtual void Draw(GameTime gameTime)
    {
        
    }

}