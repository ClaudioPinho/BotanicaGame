using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using TestMonoGame.Game;
using TestMonoGame.Game.World;
using TestMonoGame.Physics;
using TestMonoGame.Rendering;

namespace TestMonoGame;

public class MainGame : Microsoft.Xna.Framework.Game
{
    public static MainGame GameInstance;
    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    public GamePhysics Physics;

    private SpriteBatch _spriteBatch;
    private SpriteFont _fontSprite;

    private Player _player;

    private PhysicsObject _testMonkey;

    // private MeshObject _skybox;
    private PhysicsObject _plane;

    private Skybox _skybox;

    private Vector2 _reticlePosition;
    private Texture2D _reticle;

    private World _mainWorld;

    private List<GameObject> _gameObjects;
    private List<MeshObject> _meshObjects;

    private FrameCounter _frameCounter = new();

    public MainGame()
    {
        GameInstance = this;
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = false;
    }

    public void AddGameObject(GameObject gameObject)
    {
        if (_gameObjects.Contains(gameObject))
        {
            DebugUtils.PrintMessage("Trying to add game object already registered!", gameObject);
            return;
        }

        _gameObjects.Add(gameObject);
        if (gameObject is MeshObject meshObject)
        {
            _meshObjects.Add(meshObject);
        }

        if (gameObject is PhysicsObject physicsObject)
        {
            Physics.AddPhysicsObject(physicsObject);
        }

        gameObject.Initialize();
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        if (!_gameObjects.Contains(gameObject))
        {
            DebugUtils.PrintMessage("Trying to remove a game object that isn't registered!", gameObject);
            return;
        }

        if (gameObject is MeshObject meshObject)
        {
            _meshObjects.Remove(meshObject);
        }

        if (gameObject is PhysicsObject physicsObject)
        {
            Physics.RemovePhysicsObject(physicsObject);
        }

        _gameObjects.Remove(gameObject);
        gameObject.Dispose();
    }

    protected override void Initialize()
    {
        GraphicsDeviceManager.IsFullScreen = false;
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();

        // initialize the physics engine for the game
        Physics = new GamePhysics();

        DebugUtils.Initialize(GraphicsDevice);

        _meshObjects = [];
        _gameObjects = [];

        _skybox = new Skybox("Textures/Skybox/SkyRed", Content);

        _plane = new PhysicsObject("WorldPlane", true, new Vector3(200, 1f, 200f), new Vector3(0, 0, 0),
            Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0), new Vector3(100f, 100f, 1f));
        _plane.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        _plane.Model = Content.Load<Model>("Models/Primitives/plane");
        _plane.Texture = Content.Load<Texture2D>("Textures/Ground/ground-sand");
        _plane.TextureTiling = Vector2.One * 20f;
        _plane.CollisionOffset = new Vector3(0, -.5f, 0);

        _testMonkey = new PhysicsObject("The monkey", false, null, new Vector3(5f, 5f, 0f),
            Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 4, 0f));
        _testMonkey.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        _testMonkey.Model = Content.Load<Model>("Models/monkey");
        _testMonkey.Texture = Content.Load<Texture2D>("Textures/wooden-box");
        _testMonkey.TextureTiling = Vector2.One * 2f;
        _testMonkey.DiffuseColor = Color.Green;

        var cubeTest = new PhysicsObject("cube test 1", true, null, new Vector3(5, 0f, 0f));
        cubeTest.Model = Content.Load<Model>("Models/cube");
        cubeTest.DiffuseColor = Color.White;
        cubeTest.ReceiveLighting = true;
        cubeTest.DebugDrawCollision = true;
        cubeTest.CollisionSize = Vector3.One * 2;

        var cubeTest2 = new PhysicsObject("cube test 2", false, null, new Vector3(0f, 2f, 0f));
        cubeTest.Model = Content.Load<Model>("Models/cube");
        cubeTest.DiffuseColor = Color.White;
        cubeTest.ReceiveLighting = true;

        _player = new Player("Main player", new Vector3(0f, 20f, 0f), Quaternion.Identity);
        _player.DebugDrawCollision = true;

        AddGameObject(_plane);
        // AddGameObject(_testMonkey);
        AddGameObject(cubeTest);
        // AddGameObject(cubeTest2);
        AddGameObject(_player);

        _reticle = Content.Load<Texture2D>("Textures/UI/reticle");
        _reticlePosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);

        _mainWorld = new World(123456789, 20, 20);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _fontSprite = Content.Load<SpriteFont>("Fonts/myFont");
        // Debug.WriteLine(_fontSprite.Characters.Count);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _mainWorld.Update(gameTime);

        foreach (var gameObject in _gameObjects)
        {
            gameObject.Update(deltaTime);
        }

        DebugUtils.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(deltaTime);

        // update the physics simulation
        Physics.UpdatePhysics(deltaTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _skybox.Draw();

        // _mainWorld.Draw(GraphicsDevice, gameTime);

        foreach (var meshObject in _meshObjects)
        {
            meshObject.Draw(GraphicsDevice, gameTime);
        }

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_fontSprite, $"FPS: {_frameCounter.CurrentFramesPerSecond}", Vector2.Zero,
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite,
            $"Player position: {_player.Transform.Position}",
            new Vector2(0f, 20f),
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite,
            $"Camera position: {_player.Camera.Transform.Position}",
            new Vector2(0f, 40f),
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite,
            $"Player velocity: {_player.Velocity}",
            new Vector2(0f, 60f),
            Color.Yellow);
        // _spriteBatch.DrawString(_fontSprite,
        //     $"Player rotation| {_player.Transform.Rotation.ToEuler().ToString()}", new Vector2(0f, 20f),
        //     Color.Red);
        // _spriteBatch.DrawString(_fontSprite, $"Player colliding with '{_player.CollidingObjects.Count}' objects",
        //     new Vector2(0f, 20f), Color.Red);
        _spriteBatch.Draw(_reticle, _reticlePosition, Color.White);

        _spriteBatch.End();

        DebugUtils.Draw(GraphicsDevice, gameTime);

        base.Draw(gameTime);
    }
}