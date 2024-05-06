using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Game.UI;

public class UIGraphics : GameObject, IDrawable
{
    public int DrawOrder { get; }
    
    public bool Visible
    {
        get => _isVisible && IsActive;
        set => _isVisible = value;
    }
    
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;
    
    public Point AnchoredPosition
    {
        get => _anchoredPosition;
        set
        {
            _anchoredPosition = value;
            _position = new Point((int)(Anchor.X * Canvas.Width) + value.X - (int)(Size.X * Pivot.X),
                (int)(Anchor.Y * Canvas.Height) + value.Y - (int)(Size.Y * Pivot.Y));
            _drawRectangleIsDirty = true;
        }
    }

    public Point Position
    {
        get => _position;
        set
        {
            _position.X = value.X - (int)(Size.X * Pivot.X);
            _position.Y = value.Y - (int)(Size.Y * Pivot.Y);
            _anchoredPosition.X = (_position.X - (int)(Anchor.X * Canvas.Width) + (int)(Size.X * Pivot.X))
                                  / Canvas.Width;
            _anchoredPosition.Y = (_position.Y - (int)(Anchor.Y * Canvas.Height) + (int)(Size.Y * Pivot.Y))
                                  / Canvas.Height;
            _drawRectangleIsDirty = true;
        }
    }

    /// <summary>
    /// Since the canvas can be scaled this position is the true position of the graphic on the user's screen
    /// as opposed to the regular position that has no influence from the canvas scaling
    /// </summary>
    public Point RealPosition => (_position.ToVector2() * Canvas.CanvasScale).ToPoint();

    public Point Size
    {
        get => _size;
        set
        {
            _size = value;
            AnchoredPosition = _anchoredPosition;
            _drawRectangleIsDirty = true;
        }
    }
    
    /// <summary>
    /// Since the canvas can be scaled this size is the true size of the graphic on the user's screen
    /// as opposed to the regular size that has no influence from the canvas scaling
    /// </summary>
    public Point RealSize => (_size.ToVector2() * Canvas.CanvasScale).ToPoint();

    public Vector2 Pivot
    {
        get => _pivot;
        set
        {
            _pivot = value;
            _pivotIsDirty = true;
            _drawRectangleIsDirty = true;
        }
    }

    public Vector2 Anchor = Vector2.Zero;


    public Rectangle? Source = null;

    public Vector2 Scale = Vector2.One;

    public Color Color = Color.White;

    public float Rotation = 0f;

    protected readonly Canvas Canvas;

    protected Rectangle DrawRectangle { get; private set; }
    protected Rectangle RealLocation { get; private set; }

    protected Vector2 Origin { get; private set; }

    private Vector2 _pivot;
    private Point _anchoredPosition = Point.Zero;
    private Point _position = Point.Zero;
    private Point _size = new(256, 256);

    private bool _pivotIsDirty;
    private bool _drawRectangleIsDirty;

    protected readonly SpriteBatch SpriteBatch;
    
    private bool _isVisible = true;

    protected UIGraphics(Canvas canvas, SpriteBatch spriteBatch) : base("UIGraphics")
    {
        Canvas = canvas;
        DrawRectangle = new Rectangle(Position, Size);
        RealLocation = new Rectangle(RealPosition, RealSize);
        Pivot = Vector2.One * 0.5f;
        SpriteBatch = spriteBatch;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (_pivotIsDirty)
            RecalculatePivotOrigin();
        if (_drawRectangleIsDirty)
            UpdateDrawRectangle();
    }
    
    public virtual void Draw(GameTime gameTime)
    {
        if (Canvas.DrawDebugBoxes)
        {
            var destinationRectangle = new Rectangle(_position, _size);
            var textSize = MainGame.DefaultFont.MeasureString(Name);
            var labelPosition = new Point(_position.X, (int)(_position.Y - textSize.Y));
            SpriteBatch.Draw(MainGame.SinglePixelTexture, new Rectangle(labelPosition, textSize.ToPoint()),
                Color.Yellow);
            SpriteBatch.DrawString(MainGame.DefaultFont, Name, labelPosition.ToVector2(), Color.Black);
            SpriteBatch.Draw(MainGame.SquareOutlineTexture, destinationRectangle, Color.Yellow);
        }
    }

    public virtual void OnCanvasScaleChanged()
    {
        // since the canvas changed the scale we need to recalculate the real positions and size 
        _drawRectangleIsDirty = true;
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
        DrawRectangle = new Rectangle(Position + new Point((int)(Size.X * Pivot.X), (int)(Size.Y * Pivot.Y)), Size);
        RealLocation = new Rectangle(RealPosition, RealSize);
        _drawRectangleIsDirty = false;
    }
}