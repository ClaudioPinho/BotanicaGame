using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Extensions;
using TestMonoGame.Game;

namespace TestMonoGame;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _fontSprite;

    private Camera _camera;

    // World settings
    private Matrix _worldMatrix;

    private Model _testModel;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        _camera = new Camera(GraphicsDevice);
        _camera.Transform.Position.Z = -5f;

        _testModel = Content.Load<Model>("Models/test-cube");
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _fontSprite = Content.Load<SpriteFont>("Fonts/myFont");
        Debug.WriteLine(_fontSprite.Characters.Count);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (IsActive)
            UpdateInput();
        
        _camera.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (var mesh in _testModel.Meshes)
        {
            foreach (var basicEffect in mesh.Effects.Cast<BasicEffect>())
            {
                basicEffect.AmbientLightColor = Vector3.Left;
                basicEffect.View = _camera.ViewMatrix;
                // basicEffect.World = _worldMatrix;
                basicEffect.World = Matrix.Identity;
                basicEffect.Projection = _camera.ProjectionMatrix;
            }
            mesh.Draw();
        }

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_fontSprite, $"Camera position| {_camera.Transform.Position.ToString()}", Vector2.Zero,
            Color.Red);
        _spriteBatch.DrawString(_fontSprite,
            $"Camera rotation| {_camera.Transform.Rotation.ToEulerAngles().ToString()}", new Vector2(0f, 20f),
            Color.Red);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdateInput()
    {
        // todo: this sucks! I need to check a better way to reposition the mouse on the screen
        var center = new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        var delta = Mouse.GetState().Position - center;
        
        // Reset mouse position to center of window
        Mouse.SetPosition(center.X, center.Y);
        
        // Adjust camera orientation based on mouse movement
        float sensitivity = 10.1f;
        
        _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, delta.X * sensitivity);
        _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -delta.X * sensitivity);
        
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            _camera.Transform.Position += _camera.Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            _camera.Transform.Position -= _camera.Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _camera.Transform.Position += _camera.Transform.Forward * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _camera.Transform.Position -= _camera.Transform.Forward * .1f;
        }
        
        // if (Keyboard.GetState().IsKeyDown(Keys.Left))
        // {
        //     _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, .1f);
        // }
        //
        // if (Keyboard.GetState().IsKeyDown(Keys.Right))
        // {
        //     _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, -.1f);
        // }
        
        // if (Keyboard.GetState().IsKeyDown(Keys.Up))
        // {
        //     _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, .1f);
        // }
        //
        // if (Keyboard.GetState().IsKeyDown(Keys.Down))
        // {
        //     _camera.Transform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -.1f);
        // }
        
        if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
        {
            _camera.Transform.Position.Y += 1f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
        {
            _camera.Transform.Position.Y -= 1f;
        }
        // if (Keyboard.GetState().IsKeyDown(Keys.Space))
        // {
        //     orbit = !orbit;
        // }
    }
}