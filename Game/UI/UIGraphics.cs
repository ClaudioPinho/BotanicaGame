using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class UIGraphics
{
    public string Name;
    
    public Point AnchoredPosition
    {
        get => _anchoredPosition;
        set
        {
            _anchoredPosition = value;
            _position = new Point((int)(Anchor.X * Canvas.Width) + value.X, (int)(Anchor.Y * Canvas.Height) + value.Y);
            _drawRectangleIsDirty = true;
        }
    }

    public Point Position
    {
        get => _position;
        set
        {
            _position = value;
            _anchoredPosition = new Point((value.X - (int)(Anchor.X * Canvas.Width)) / Canvas.Width,
                (value.Y - (int)(Anchor.Y * Canvas.Height)) / Canvas.Height);
            _drawRectangleIsDirty = true;
        }
    }

    public Point Size
    {
        get => _size;
        set
        {
            _size = value;
            _drawRectangleIsDirty = true;
        }
    }

    public Vector2 Pivot
    {
        get => _pivot;
        set
        {
            _pivot = value;
            RecalculatePivotOrigin();
        }
    }
    
    public Vector2 Anchor = Vector2.Zero;
    

    public Rectangle? Source = null;
    
    public Vector2 Scale = Vector2.One;
    
    public Color Color = Color.White;
    
    public float Rotation = 0f;

    protected readonly Canvas Canvas;

    protected Rectangle DrawRectangle { get; private set; }

    protected Vector2 Origin { get; private set; }
    
    private Vector2 _pivot;
    private Point _anchoredPosition = Point.Zero;
    private Point _position = Point.Zero;
    private Point _size = new(256, 256);
    private bool _drawRectangleIsDirty;

    public UIGraphics(Canvas canvas)
    {
        Canvas = canvas;
        DrawRectangle = new Rectangle(Position, Size);
        Pivot = Vector2.One * 0.5f;
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (_drawRectangleIsDirty)
            UpdateDrawRectangle();
    }

    protected void RecalculatePivotOrigin()
    {
        Origin = GetPivotOrigin();
    }
    
    protected virtual Vector2 GetPivotOrigin()
    {
        return new Vector2(Size.X * _pivot.X, Size.Y * _pivot.Y);
    }

    private void UpdateDrawRectangle()
    {
        DrawRectangle = new Rectangle(Position, Size);
        _drawRectangleIsDirty = false;
    }

}