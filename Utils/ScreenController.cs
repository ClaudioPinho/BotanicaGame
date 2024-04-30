using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BotanicaGame.Utils;

public class ScreenController(GraphicsDeviceManager deviceManager, GameWindow gameWindow)
{
    // todo: these actions should probably be handled by a main event system
    public Action<int, int> OnScreenResolutionChanged;

    bool _isFullscreen = false;
    bool _isBorderless = false;
    int _width = 0;
    int _height = 0;

    public void ToggleFullscreen()
    {
        bool oldIsFullscreen = _isFullscreen;

        if (_isBorderless)
        {
            _isBorderless = false;
        }
        else
        {
            _isFullscreen = !_isFullscreen;
        }

        ApplyFullscreenChange(oldIsFullscreen);
        OnScreenResolutionChanged?.Invoke(deviceManager.PreferredBackBufferWidth,
            deviceManager.PreferredBackBufferHeight);
    }

    public void ToggleBorderless()
    {
        bool oldIsFullscreen = _isFullscreen;

        _isBorderless = !_isBorderless;
        _isFullscreen = _isBorderless;

        ApplyFullscreenChange(oldIsFullscreen);
        OnScreenResolutionChanged?.Invoke(deviceManager.PreferredBackBufferWidth,
            deviceManager.PreferredBackBufferHeight);
    }

    private void ApplyFullscreenChange(bool oldIsFullscreen)
    {
        if (_isFullscreen)
        {
            if (oldIsFullscreen)
            {
                ApplyHardwareMode();
            }
            else
            {
                SetFullscreen();
            }
        }
        else
        {
            UnsetFullscreen();
        }
    }

    private void ApplyHardwareMode()
    {
        deviceManager.HardwareModeSwitch = !_isBorderless;
        deviceManager.ApplyChanges();
    }

    private void SetFullscreen()
    {
        _width = gameWindow.ClientBounds.Width;
        _height = gameWindow.ClientBounds.Height;

        deviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        deviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        deviceManager.HardwareModeSwitch = !_isBorderless;

        deviceManager.IsFullScreen = true;
        deviceManager.ApplyChanges();
    }

    private void UnsetFullscreen()
    {
        deviceManager.PreferredBackBufferWidth = _width;
        deviceManager.PreferredBackBufferHeight = _height;
        deviceManager.IsFullScreen = false;
        deviceManager.ApplyChanges();
    }
}