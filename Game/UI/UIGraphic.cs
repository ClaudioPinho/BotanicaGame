using System;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Game.UI;

public class UIGraphic : GameObject, IDrawable
{
    public int DrawOrder { get; set; }

    public bool Visible
    {
        get
        {
            if (_parentDrawable != null)
                return _isVisible && IsActive && _parentDrawable.Visible;
            return _isVisible && IsActive;
        }
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
            _position = new Point((int)(Anchor.X * CanvasWidth) + value.X - (int)(Size.X * Pivot.X),
                (int)(Anchor.Y * CanvasHeight) + value.Y - (int)(Size.Y * Pivot.Y));
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
            _anchoredPosition.X = (_position.X - (int)(Anchor.X * Canvas.Width) + (int)(Size.X * Pivot.X)) / CanvasWidth;
            _anchoredPosition.Y = (_position.Y - (int)(Anchor.Y * Canvas.Height) + (int)(Size.Y * Pivot.Y)) / CanvasHeight;
            _drawRectangleIsDirty = true;
        }
    }

    /// <summary>
    /// Since the canvas can be scaled this position is the true position of the graphic on the user's screen
    /// as opposed to the regular position that has no influence from the canvas scaling
    /// </summary>
    public Point RealPosition => (_position.ToVector2() * CanvasScale).ToPoint();

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
    public Point RealSize => (_size.ToVector2() * (Canvas?.CanvasScale ?? Vector2.One)).ToPoint();

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

    protected Canvas Canvas;

    protected Rectangle DrawRectangle { get; private set; }
    protected Rectangle RealLocation { get; private set; }

    protected Vector2 Origin { get; private set; }

    protected int CanvasWidth => Canvas?.Width ?? MainGame.GameInstance.GraphicsDevice.Viewport.Width;
    protected int CanvasHeight => Canvas?.Width ?? MainGame.GameInstance.GraphicsDevice.Viewport.Height;
    protected Vector2 CanvasScale => Canvas?.CanvasScale ?? Vector2.One;

    private Vector2 _pivot;
    private Point _anchoredPosition = Point.Zero;
    private Point _position = Point.Zero;
    private Point _size = new(256, 256);

    private bool _pivotIsDirty;
    private bool _drawRectangleIsDirty;

    private bool _isVisible = true;

    private IDrawable _parentDrawable;

    public UIGraphic(string id) : base(id)
    {
        DrawRectangle = Rectangle.Empty;
        RealLocation = Rectangle.Empty;
        Pivot = Vector2.One * 0.5f;
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
        if (Canvas == null) return;
        if (!Canvas.DrawDebugBoxes) return;
        var destinationRectangle = new Rectangle(_position, _size);
        var textSize = MainGame.DefaultSpriteFont.MeasureString(Name);
        var labelPosition = new Point(_position.X, (int)(_position.Y - textSize.Y));
        Canvas.SpriteBatch.Draw(MainGame.SinglePixelTexture, new Rectangle(labelPosition, textSize.ToPoint()),
            Color.Yellow);
        Canvas.SpriteBatch.DrawString(MainGame.DefaultSpriteFont, Name, labelPosition.ToVector2(), Color.Black);
        Canvas.SpriteBatch.Draw(MainGame.SquareOutlineTexture, destinationRectangle, Color.Yellow);
    }

    public virtual void OnCanvasScaleChanged()
    {
        // since the canvas changed the scale we need to recalculate the real positions and size 
        _drawRectangleIsDirty = true;
    }

    protected virtual void SetCanvas(Canvas canvas)
    {
        if (canvas == null) return;
        // if we are changing the canvas we need to check if we had one previously and remove this graphic from it
        if (canvas != Canvas && Canvas != null)
        {
            Canvas.RemoveUIGraphic(this);
        }
        Canvas = canvas;
        Canvas.AddUIGraphic(this);
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

    protected override void UpdateParent(GameObject parentObject)
    {
        base.UpdateParent(parentObject);
        if (parentObject == null) return;
        // automatically set the canvas if this graphic is being parented to one
        SetCanvas(parentObject.FindTypeInParents<Canvas>());
        _parentDrawable = parentObject.FindTypeInParents<IDrawable>();
    }

    private void UpdateDrawRectangle()
    {
        DrawRectangle = new Rectangle(Position + new Point((int)(Size.X * Pivot.X), (int)(Size.Y * Pivot.Y)), Size);
        RealLocation = new Rectangle(RealPosition, RealSize);
        _drawRectangleIsDirty = false;
    }
}