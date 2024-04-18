using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIGraphics
{
    public Point Position => Destination.Location;

    public Rectangle Destination = new(0,0,100,100);
    public Rectangle? Source = null;
    
    public Vector2 Origin = Vector2.Zero;

    public Vector2 Scale = Vector2.One;
    public Color Color = Color.White;

    public float Rotation = 0f;

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        
        
    }

}