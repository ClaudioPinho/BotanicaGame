using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TestMonoGame.Game;

public class Player : GameObject
{
    public Player()
    {
        Components.Add(new Camera(Transform));
    }

    public override void Update()
    {
        base.Update();
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            Transform.Position -= Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            Transform.Position += Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            Transform.Position += Transform.Forward * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            Transform.Position -= Transform.Forward * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.E))
        {
            Transform.Position.Y -= .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Q))
        {
            Transform.Position.Y += .1f;
        }

        Transform.Position.Y = 2f;

        if (MainGame.GameInstance.IsActive)
            UpdateInput();
    }

    private float yaw = 0f;
    private float pitch = 0f;

    private const float _maxPitch = MathF.PI / 2 - 0.01f; // Just shy of 90 degrees
    private const float _minPitch = -_maxPitch;
    
    private void UpdateInput()
    {
        var center = new Point(MainGame.GameInstance.Window.ClientBounds.Width / 2,
            MainGame.GameInstance.Window.ClientBounds.Height / 2);
        var delta = Mouse.GetState().Position - center;

        // Reset mouse position to center of window
        Mouse.SetPosition(center.X, center.Y);

        // Adjust camera orientation based on mouse movement
        var sensitivity = 0.005f;

        // Calculate rotation angles based on mouse movement
        yaw -= delta.X * sensitivity;
        pitch += delta.Y * sensitivity;
        
        // Clamp the pitch angle to prevent camera inversion
        pitch = MathHelper.Clamp(pitch, _minPitch, _maxPitch);

        Transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0f);
    }
}