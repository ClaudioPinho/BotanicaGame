using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using Velentr.Font;

namespace TestMonoGame.Game.UI;

public class Canvas : GameObject, IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;

    public const bool DrawDebugBoxes = false;

    public Color BackgroundColor = new(0, 0, 0, 0);

    public int Width => _fullViewportRectangle.Width;
    public int Height => _fullViewportRectangle.Height;

    public FontManager FontManager { get; }

    public Font DefaultFont { get; private set; }
    public string DefaultFontResource { get; private set; }

    private SpriteBatch _spriteBatch = new(MainGame.GraphicsDeviceManager.GraphicsDevice);
    private List<UIGraphics> _graphicsToDraw = [];
    private List<UIInteractable> _interactableUIElements = [];

    private int _virtualWidth = 1280;
    private int _virtualHeight = 720;

    private Rectangle _fullViewportRectangle = new(0, 0, 1280, 720);

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
        base.Update(deltaTime);
        CheckUIInteractions();
        foreach (var graphic in _graphicsToDraw)
        {
            graphic.Update(deltaTime);
        }
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

        foreach (var graphic in _graphicsToDraw)
        {
            if (!graphic.ShouldDraw) continue;
            graphic.Draw(_spriteBatch, gameTime);
        }

        _spriteBatch.End();
    }

    public void SetRenderResolution(int width, int height)
    {
        _virtualWidth = width;
        _virtualHeight = height;
    }

    private void CheckUIInteractions()
    {
        var mousePosition = Mouse.GetState().Position;
        var beingInteracted = _interactableUIElements
            .Where(interactable => interactable.InteractionArea.Contains(mousePosition)).ToList();
        foreach (var interacted in beingInteracted)
        {
            interacted.OnHoverStart();
        }
    }
}