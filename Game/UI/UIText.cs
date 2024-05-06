using System;
using BotanicaGame.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Velentr.Font;

namespace BotanicaGame.Game.UI;

public class UIText : UIInteractable
{
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _textIsDirty = true;
        }
    }

    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            _fontSizeIsDirty = true;
        }
    }

    public Font Font
    {
        get => _font;
        set
        {
            _font = value;
            _textIsDirty = true;
        }
    }

    /// <summary>
    /// Exposes a way to load new fonts from the scene files
    /// todo: should I think of a different way to load fonts from the scene data?
    /// </summary>
    public string FontResource
    {
        set
        {
            var fontResource = $"Content/{value}";
            try
            {
                Font = _canvas.FontManager.GetFont(fontResource, _fontSize);
                _fontResource = fontResource;
            }
            catch (Exception e)
            {
                DebugUtils.PrintError($"Couldn't load font resource from '{fontResource}'");
                DebugUtils.PrintException(e);
                Font = _canvas.DefaultFont;
            }
        }
    }

    public bool UseMarkdown
    {
        get => _useMarkdown;
        set
        {
            _useMarkdown = value;
            _textIsDirty = true;
        }
    }

    public bool AutoResize
    {
        get => _autoResize;
        set
        {
            _autoResize = value;
            _textSizeIsDirty = true;
        }
    }

    private string _text;
    private int _fontSize;
    private Font _font;
    private string _fontResource;
    private Text _textToRender;
    private bool _useMarkdown;
    private bool _autoResize;

    private bool _textIsDirty;
    private bool _fontSizeIsDirty;
    private bool _textSizeIsDirty;

    private readonly Canvas _canvas;

    public UIText(Canvas canvas, SpriteBatch spriteBatch) : base(canvas, spriteBatch)
    {
        _canvas = canvas;
        _text = "My Text";
        _fontResource = _canvas.DefaultFontResource;
        _fontSize = _canvas.DefaultFont.Size;
        Font = _canvas.DefaultFont;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (_fontSizeIsDirty)
        {
            UpdateFontSize();
            _fontSizeIsDirty = false;
        }

        if (_textIsDirty)
        {
            UpdateText();
            _textIsDirty = false;
        }

        if (_textSizeIsDirty && _autoResize)
        {
            UpdateTextSize();
            _textSizeIsDirty = false;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        if (_useMarkdown)
        {
            SpriteBatch.DrawStringWithMarkdown(_textToRender, Position.ToVector2(), Color, Rotation, Origin, Scale,
                SpriteEffects.None, 0);
        }
        else
        {
            SpriteBatch.DrawString(_textToRender, Position.ToVector2(), Color, Rotation, Origin, Scale,
                SpriteEffects.None, 0);
        }
    }

    protected override Vector2 GetPivotOrigin() => Vector2.Zero;

    private void UpdateFontSize()
    {
        if (string.IsNullOrEmpty(_fontResource))
        {
            DebugUtils.PrintWarning("Trying to resize font without resource, ignoring...", this);
        }
        else
        {
            _font = _canvas.FontManager.GetFont(_fontResource, _fontSize);
        }
    }

    private void UpdateText()
    {
        // todo: I am worried about the performance of caching the glyphs
        _textToRender = _font.MakeText(_text, _useMarkdown);
        _textSizeIsDirty = true;
    }

    private void UpdateTextSize()
    {
        Size = _textToRender.Size.ToPoint();
    }
}