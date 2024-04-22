using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Game.UI;

public class Canvas(string name) : GameObject(name), IDrawable
{
    public int DrawOrder { get; }
    public bool Visible { get; }
    public event EventHandler<EventArgs> DrawOrderChanged;
    public event EventHandler<EventArgs> VisibleChanged;

    public List<UIGraphics> GraphicsList => _graphicsToDraw;
    
    public Color BackgroundColor = new(0, 0, 0, 0);

    public int Width => _fullViewportRectangle.Width;
    public int Height => _fullViewportRectangle.Height;
    
    private SpriteBatch _spriteBatch = new(MainGame.GraphicsDeviceManager.GraphicsDevice);
    private List<UIGraphics> _graphicsToDraw = [];
    
    private int _virtualWidth = 1280;
    private int _virtualHeight = 720;

    private Rectangle _fullViewportRectangle = new(0, 0, 1280, 720);

    private Matrix _scaleMatrix;

    public void AddUIGraphic(UIGraphics uiGraphics)
    {
        if (!_graphicsToDraw.Contains(uiGraphics))
            _graphicsToDraw.Add(uiGraphics);
    }
    
    public void RemoveUIGraphic(UIGraphics uiGraphics)
    {
        _graphicsToDraw.Remove(uiGraphics);
    }
    
    public void Draw(GameTime gameTime)
    {
        // _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: );
        _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
        
        _spriteBatch.Draw(MainGame.SinglePixelTexture, _fullViewportRectangle,  BackgroundColor);
        
        foreach (var graphic in _graphicsToDraw)
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
}