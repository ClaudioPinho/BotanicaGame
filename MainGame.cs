using System;
using System.Collections.Generic;
using System.Linq;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using TestMonoGame.Game;
using TestMonoGame.Game.World;
using TestMonoGame.Physics;
using TestMonoGame.Rendering;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/monogame-collision-detection

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

    public void AddGameObject(GameObject newGameObject)
    {
        if (_gameObjects.Contains(newGameObject))
        {
            DebugUtils.PrintMessage("Trying to add game object already registered!", newGameObject);
            return;
        }
        _gameObjects.Add(newGameObject);
        if (newGameObject is MeshObject meshObject)
        {
            _meshObjects.Add(meshObject);
        }
        newGameObject.Initialize();
    }

    public void RemoveGameObject(GameObject gameObjectToRemove)
    {
        if (!_gameObjects.Contains(gameObjectToRemove))
        {
            DebugUtils.PrintMessage("Trying to remove a game object that isn't registered!", gameObjectToRemove);
            return;
        }
        if (gameObjectToRemove is MeshObject meshObject)
        {
            _meshObjects.Remove(meshObject);
        }
        _gameObjects.Remove(gameObjectToRemove);
        gameObjectToRemove.Dispose();
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

        _plane = CreateNewGameObject<PhysicsObject>(new Vector3(0, -2f, 0),
            Quaternion.CreateFromYawPitchRoll(0f, -MathF.PI / 2, 0f),
            new Vector3(100f, 100f, 1f));
        _plane.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        _plane.Model = Content.Load<Model>("Models/Primitives/plane");
        _plane.Texture = Content.Load<Texture2D>("Textures/Ground/ground-sand");
        _plane.TextureTiling = Vector2.One * 20f;
        _plane.RigidBody.Shape = new BoxShape(200f, .1f, 200f);
        _plane.RigidBody.Shape.UpdateShape();
        _plane.RigidBody.IsStatic = true;
        _plane.UsePhysicsRotation = false;

        _testMonkey = CreateNewGameObject<PhysicsObject>(new Vector3(5f, 5f, 0f),
            Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 4, 0f));
        _testMonkey.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        _testMonkey.Model = Content.Load<Model>("Models/monkey");
        _testMonkey.Texture = Content.Load<Texture2D>("Textures/wooden-box");
        _testMonkey.TextureTiling = Vector2.One * 2f;
        _testMonkey.DiffuseColor = Color.Green;
        _testMonkey.RigidBody.Shape = new SphereShape(1f);
        _testMonkey.RigidBody.Shape.UpdateShape();
        _testMonkey.RigidBody.Mass = 5f;

        var cubeTest = CreateNewGameObject<PhysicsObject>(new Vector3(0f, 1f, 0f));
        cubeTest.Model = Content.Load<Model>("Models/cube");
        cubeTest.RigidBody.IsStatic = true;
        cubeTest.DiffuseColor = Color.White;
        cubeTest.ReceiveLighting = true;
        
        var cubeTest2 = CreateNewGameObject<PhysicsObject>(new Vector3(0f, 2f, 0f));
        cubeTest2.Model = Content.Load<Model>("Models/cube");
        cubeTest2.RigidBody.IsStatic = true;
        cubeTest2.DiffuseColor = Color.White;
        cubeTest2.ReceiveLighting = true;

        _player = new Player();
        _player = CreateNewGameObject<Player>(new Vector3(0f, 20f, 0f), Quaternion.Identity);
        _player.RigidBody.Mass = 90f;

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

        _mainWorld.Update(gameTime);
        
        foreach (var gameObject in _gameObjects)
        {
            gameObject.Update(gameTime);
        }

        DebugUtils.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(deltaTime);

        // update the physics simulation
        Physics.UpdatePhysics(gameTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _skybox.Draw();

        _mainWorld.Draw(GraphicsDevice, gameTime);
        
        foreach (var meshObject in _meshObjects)
        {
            meshObject.Draw(GraphicsDevice, gameTime);
        }

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_fontSprite, $"FPS: {_frameCounter.CurrentFramesPerSecond}", Vector2.Zero,
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite,
            $"Player restitution: {_player.RigidBody.Material.Restitution} data: {_player.Transform} velocity: {MathF.Round(_player.RigidBody.LinearVelocity.Length(), 2):00.00}",
            new Vector2(0f, 20f),
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