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
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;


    public bool DrawDebugBoxes = false;

    public UIInteractable CurrHoveredElement { get; private set; }

    public Color BackgroundColor = new(0, 0, 0, 0);


    public int Width => _fullViewportRectangle.Width;
    public int Height => _fullViewportRectangle.Height;


    public FontManager FontManager { get; }
    public Font DefaultFont { get; private set; }
    public string DefaultFontResource { get; private set; }


    private readonly SpriteBatch _spriteBatch = new(MainGame.GraphicsDeviceManager.GraphicsDevice);
    private readonly List<UIGraphics> _graphicsToDraw = [];
    private List<UIInteractable> _interactableUIElements = [];

    private int _virtualWidth = 1280;
    private int _virtualHeight = 720;

    private List<UIInteractable> _beingHovered;
    private List<UIInteractable> _previouslyHovered;
    private MouseState _previousMouseState;
    private MouseState _mouseState;
    private Point _mousePosition;

    private readonly Rectangle _fullViewportRectangle = new(0, 0, 1280, 720);

    private Matrix _scaleMatrix;

    public Canvas(string name) : base(name)
    {
        FontManager = new FontManager(MainGame.GraphicsDeviceManager.GraphicsDevice);
        DefaultFontResource = "Content/Fonts/BeonMedium.ttf";
        DefaultFont = FontManager.GetFont(DefaultFontResource, 32);
    }

    public void AddUIGraphic(UIGraphics uiGraphics)
    {
        if (!_graphicsToDraw.Contains(uiGraphics))
        {
            if (uiGraphics is UIInteractable uiInteractable)
                _interactableUIElements.Add(uiInteractable);
            _graphicsToDraw.Add(uiGraphics);
        }
    }

    public void RemoveUIGraphic(UIGraphics uiGraphics)
    {
        if (uiGraphics is UIInteractable uiInteractable)
            _interactableUIElements.Remove(uiInteractable);
        _graphicsToDraw.Remove(uiGraphics);
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

    public T GetGraphicByName<T>(string name) where T : UIGraphics
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
        // _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: );
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

        _spriteBatch.Draw(MainGame.SinglePixelTexture, _fullViewportRectangle, BackgroundColor);

        foreach (var graphic in _graphicsToDraw.Where(graphic => graphic.ShouldDraw))
        {
            graphic.Draw(_spriteBatch, gameTime);
        }

        _spriteBatch.End();
    }

    public void SetRenderResolution(int width, int height)
    {
        _virtualWidth = width;
        _virtualHeight = height;
    }

    private void CheckUIHovers()
    {
        _mousePosition = _mouseState.Position;

        _beingHovered = _interactableUIElements
            .Where(x => x.ShouldDraw && x.CanBeInteracted && x.InteractionArea.Contains(_mousePosition))
            .ToList();
        _previouslyHovered = _interactableUIElements
            .Where(x => x.ShouldDraw && x.CanBeInteracted && x.IsBeingHovered)
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
}