using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Extensions;
using TestMonoGame.Game;
using TestMonoGame.Rendering;

namespace TestMonoGame;

public class MainGame : Microsoft.Xna.Framework.Game
{
    public static MainGame GameInstance;

    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    private SpriteBatch _spriteBatch;
    private SpriteFont _fontSprite;

    // World settings
    private Matrix _worldMatrix;

    private Model _testModel;
    private Model _sphereModel;
    private PrimitivePlane _plane;

    private Texture2D _skyboxTexture;
    private Texture2D _grassTexture;
    private Texture2D[] skyboxTextures;

    private Player _player;
    private Camera _playerCamera;

    private List<GameObject> _gameObjects;

    public MainGame()
    {
        GameInstance = this;
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        GraphicsDeviceManager.IsFullScreen = false;
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();
        
        _gameObjects = new List<GameObject>();

        _player = new Player();
        _playerCamera = _player.GetComponent<Camera>();

        _gameObjects.Add(_player);

        _testModel = Content.Load<Model>("Models/test-cube");
        _sphereModel = Content.Load<Model>("Models/sphere-inv-normal");

        skyboxTextures = new Texture2D[6];
        skyboxTextures[0] = Content.Load<Texture2D>("Textures/Skybox/skybox_front");
        skyboxTextures[1] = Content.Load<Texture2D>("Textures/Skybox/skybox_back");
        skyboxTextures[2] = Content.Load<Texture2D>("Textures/Skybox/skybox_left");
        skyboxTextures[3] = Content.Load<Texture2D>("Textures/Skybox/skybox_right");
        skyboxTextures[4] = Content.Load<Texture2D>("Textures/Skybox/skybox_up");
        skyboxTextures[5] = Content.Load<Texture2D>("Textures/Skybox/skybox_down");

        _skyboxTexture = Content.Load<Texture2D>("Textures/Skybox/sky-sphere-1");
        _grassTexture = Content.Load<Texture2D>("Textures/Ground/Grass_02");

        _plane = new PrimitivePlane(GraphicsDevice);

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

        foreach (var gameObject in _gameObjects)
        {
            gameObject.Update();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        DrawModel(_sphereModel, Matrix.CreateScale(200f) * Matrix.CreateRotationX(MathHelper.Pi), _skyboxTexture);

        _plane.Draw(_playerCamera, _grassTexture);
        DrawModel(_testModel, Matrix.Identity, skyboxTextures[0]);

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_fontSprite, $"Player position| {_player.Transform.Position.ToString()}", Vector2.Zero,
            Color.Red);
        _spriteBatch.DrawString(_fontSprite,
            $"Player rotation| {_player.Transform.Rotation.ToEulerAngles().ToString()}", new Vector2(0f, 20f),
            Color.Red);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawModel(Model model, Matrix world, Texture2D texture2D = null)
    {
        foreach (var mesh in model.Meshes)
        {
            // MeshUtils.InvertNormals(mesh);

            foreach (var basicEffect in mesh.Effects.Cast<BasicEffect>())
            {
                if (texture2D != null)
                {
                    basicEffect.TextureEnabled = true;
                    basicEffect.Texture = texture2D;
                }
                else
                {
                    basicEffect.TextureEnabled = false;
                }

                // effect.EnableDefaultLighting();
                // effect.DiffuseColor = Color.Gray.ToVector3();
                basicEffect.AmbientLightColor = Color.Wheat.ToVector3();

                basicEffect.World = world;
                basicEffect.View = _playerCamera.ViewMatrix;
                basicEffect.Projection = _playerCamera.ProjectionMatrix;
            }

            mesh.Draw();
        }
    }
}