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

    private SpriteBatch _spriteBatch = new(MainGame.GraphicsDeviceManager.GraphicsDevice);
    private List<UIGraphics> _graphicsToDraw = [];

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
        _spriteBatch.Begin();
        
        foreach (var graphic in _graphicsToDraw)
        {
            graphic.Draw(_spriteBatch, gameTime);
        }
        
        _spriteBatch.End();
    }
}