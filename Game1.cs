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

    private Texture2D[] skyboxTextures;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        _camera = new Camera(GraphicsDevice);
        _camera.Transform.Position.Z = -5f;

        _testModel = Content.Load<Model>("Models/test-cube");

        skyboxTextures = new Texture2D[6];
        skyboxTextures[0] = Content.Load<Texture2D>("Textures/Skybox/skybox_front");
        skyboxTextures[1] = Content.Load<Texture2D>("Textures/Skybox/skybox_back");
        skyboxTextures[2] = Content.Load<Texture2D>("Textures/Skybox/skybox_left");
        skyboxTextures[3] = Content.Load<Texture2D>("Textures/Skybox/skybox_right");
        skyboxTextures[4] = Content.Load<Texture2D>("Textures/Skybox/skybox_up");
        skyboxTextures[5] = Content.Load<Texture2D>("Textures/Skybox/skybox_down");

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
        GraphicsDevice.Clear(Color.White);

        foreach (var mesh in _testModel.Meshes)
        {
            int i = 0;
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.Texture = skyboxTextures[i++];
                effect.TextureEnabled = true;
                
                effect.World = Matrix.Identity;
                effect.View = _camera.ViewMatrix;
                effect.Projection = _camera.ProjectionMatrix;
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

    private float yaw = 0f;
    private float pitch = 0f;
    
    private void UpdateInput()
    {
        // var mouseNow = Mouse.GetState();

        var center = new Point(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        var delta = Mouse.GetState().Position - center;

        // Reset mouse position to center of window
        Mouse.SetPosition(center.X, center.Y);

        // Adjust camera orientation based on mouse movement
        float sensitivity = 0.005f;

        // Calculate rotation angles based on mouse movement
        yaw -= delta.X * sensitivity;
        pitch -= delta.Y * sensitivity;
        
        _camera.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0f);

        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            _camera.Transform.Position -= _camera.Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            _camera.Transform.Position += _camera.Transform.Right * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            _camera.Transform.Position += _camera.Transform.Forward * .1f;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            _camera.Transform.Position -= _camera.Transform.Forward * .1f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.E))
        {
            _camera.Transform.Position.Y -= .1f;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.Q))
        {
            _camera.Transform.Position.Y += .1f;
        }
    }
}