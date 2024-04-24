using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Game.UI;

public class UIGraphics
{
    public string Name;

    public bool ShouldDraw = true;

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

    public UIGraphics(Canvas canvas)
    {
        Canvas = canvas;
        DrawRectangle = RealLocation = new Rectangle(Position, Size);
        Pivot = Vector2.One * 0.5f;
    }

    public virtual void Update(float deltaTime)
    {
        if (_pivotIsDirty)
            RecalculatePivotOrigin();
        if (_drawRectangleIsDirty)
            UpdateDrawRectangle();
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (Canvas.DrawDebugBoxes)
        {
            var destinationRectangle = new Rectangle(_position, _size);
            var textSize = MainGame.DefaultFont.MeasureString(Name);
            var labelPosition = new Point(_position.X, (int)(_position.Y - textSize.Y));
            spriteBatch.Draw(MainGame.SinglePixelTexture, new Rectangle(labelPosition, textSize.ToPoint()),
                Color.Yellow);
            spriteBatch.DrawString(MainGame.DefaultFont, Name, labelPosition.ToVector2(), Color.Black);
            spriteBatch.Draw(MainGame.SquareOutlineTexture, destinationRectangle, Color.Yellow);
        }
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
        RealLocation = new Rectangle(Position, Size);
        // DrawRectangle = new Rectangle(Position + _size * _pivot.ToPoint(), Size);
        _drawRectangleIsDirty = false;
    }
}