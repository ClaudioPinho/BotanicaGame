using System;
using System.Collections.Generic;
using System.Linq;
using BotanicaGame.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Velentr.Font;

namespace BotanicaGame.Game.UI;

public class Canvas : GameObject, IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; set; } = true;
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;


    public bool DrawDebugBoxes = false;

    public UIInteractable CurrHoveredElement { get; private set; }

    public Color BackgroundColor = new(0, 0, 0, 0);


    public int Width => _fullViewportRectangle.Width;
    public int Height => _fullViewportRectangle.Height;

    public Vector2 CanvasScale => _canvasScale;

    public FontManager FontManager { get; }
    public Font DefaultFont { get; private set; }
    public string DefaultFontResource { get; private set; }

    public SpriteBatch SpriteBatch { get; } = new(MainGame.GraphicsDeviceManager.GraphicsDevice);
    
    private readonly List<UIGraphic> _graphicsToDraw = [];
    private List<UIInteractable> _interactableUIElements = [];

    private int _virtualWidth;
    private int _virtualHeight;

    private List<UIInteractable> _beingHovered;
    private List<UIInteractable> _previouslyHovered;
    private MouseState _previousMouseState;
    private MouseState _mouseState;
    private Point _mousePosition;

    private readonly Rectangle _fullViewportRectangle = new(0, 0, 1280, 720);

    private Vector2 _canvasScale;
    private Matrix _scaleMatrix;

    public Canvas(string id) : base(id)
    {
        SetVirtualResolution(1280, 720);

        FontManager = new FontManager(MainGame.GraphicsDeviceManager.GraphicsDevice);
        DefaultFontResource = "Content/Fonts/BeonMedium.ttf";
        DefaultFont = FontManager.GetFont(DefaultFontResource, 32);
        MainGame.GameInstance.ScreenController.OnScreenResolutionChanged += OnScreenResolutionChanged;
        
        _canvasScale = Vector2.One;
    }

    public void AddUIGraphic(UIGraphic uiGraphic)
    {
        if (!_graphicsToDraw.Contains(uiGraphic))
        {
            if (uiGraphic is UIInteractable uiInteractable)
                _interactableUIElements.Add(uiInteractable);
            _graphicsToDraw.Add(uiGraphic);
        }
    }

    public void RemoveUIGraphic(UIGraphic uiGraphic)
    {
        if (uiGraphic is UIInteractable uiInteractable)
            _interactableUIElements.Remove(uiInteractable);
        _graphicsToDraw.Remove(uiGraphic);
    }

    public override void Update(float deltaTime)
    {
        _mouseState = Mouse.GetState();
        CheckUIHovers();
        CheckUIMouseInteraction();
        foreach (var graphic in _graphicsToDraw)
        {
            graphic.Update(deltaTime);
        }

        _previousMouseState = _mouseState;

        base.Update(deltaTime);
    }

    public T GetGraphicByName<T>(string name) where T : UIGraphic
    {
        var validGraphic = _graphicsToDraw.FirstOrDefault(x => x.Name == name);

        if (validGraphic == null)
        {
            DebugUtils.PrintError($"No graphic with name '{name}' found");
            return default;
        }

        if (validGraphic.GetType().IsAssignableTo(typeof(T)))
        {
            return validGraphic as T;
        }

        DebugUtils.PrintError($"Graphic with '{name}' found but it's not of type '{typeof(T).Name}'");
        return default;
    }

    public void Draw(GameTime gameTime)
    {
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
            transformMatrix: _scaleMatrix);

        SpriteBatch.Draw(MainGame.SinglePixelTexture, _fullViewportRectangle, BackgroundColor);

        foreach (var graphic in _graphicsToDraw.Where(graphic => graphic.Visible))
        {
            graphic.Draw(gameTime);
        }

        SpriteBatch.End();
    }

    public void SetVirtualResolution(int width, int height)
    {
        _virtualWidth = width;
        _virtualHeight = height;
        OnScreenResolutionChanged(MainGame.GraphicsDeviceManager.PreferredBackBufferWidth,
            MainGame.GraphicsDeviceManager.PreferredBackBufferHeight);
    }

    private void CheckUIHovers()
    {
        _mousePosition = _mouseState.Position;

        _beingHovered = _interactableUIElements
            .Where(x => x.Visible && x.CanBeInteracted && x.InteractionArea.Contains(_mousePosition))
            .ToList();
        _previouslyHovered = _interactableUIElements
            .Where(x => x.Visible && x.CanBeInteracted && x.IsBeingHovered)
            .ToList();

        if (_beingHovered.Count > 0)
        {
            CurrHoveredElement = _beingHovered.Last();
            _previouslyHovered.Remove(CurrHoveredElement);

            if (CurrHoveredElement is { IsBeingHovered: false })
            {
                CurrHoveredElement.OnHoverStart();
            }
        }
        else
        {
            CurrHoveredElement = null;
        }

        foreach (var previousHovered in _previouslyHovered)
        {
            previousHovered.OnHoverEnd();
        }
    }

    private void CheckUIMouseInteraction()
    {
        if (CurrHoveredElement == null)
            return;
        if ((_mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton != ButtonState.Pressed) ||
            (_mouseState.RightButton == ButtonState.Pressed &&
             _previousMouseState.RightButton != ButtonState.Pressed) ||
            (_mouseState.MiddleButton == ButtonState.Pressed &&
             _previousMouseState.MiddleButton != ButtonState.Pressed))
        {
            CurrHoveredElement.OnSelected(_mouseState);
        }

        if ((_mouseState.LeftButton == ButtonState.Released &&
             _previousMouseState.LeftButton != ButtonState.Released) ||
            (_mouseState.RightButton == ButtonState.Released &&
             _previousMouseState.RightButton != ButtonState.Released) ||
            (_mouseState.MiddleButton == ButtonState.Released &&
             _previousMouseState.MiddleButton != ButtonState.Released))
        {
            CurrHoveredElement.OnDeselected(_mouseState);
        }
    }

    private void OnScreenResolutionChanged(int width, int height)
    {
        _canvasScale.X = (float)width / _virtualWidth;
        _canvasScale.Y = (float)height / _virtualHeight;
        _scaleMatrix = Matrix.CreateScale(_canvasScale.X, _canvasScale.Y, 1.0f);
        _graphicsToDraw.ForEach(x => x.OnCanvasScaleChanged());
    }
}