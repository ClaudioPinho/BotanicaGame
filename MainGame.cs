using System;
using System.Collections.Generic;
using Jitter.Collision.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Debug;
using TestMonoGame.Game;
using TestMonoGame.Physics;
using TestMonoGame.Rendering;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/monogame-collision-detection

public class MainGame : Microsoft.Xna.Framework.Game
{
    public static MainGame GameInstance;

    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    private SpriteBatch _spriteBatch;
    private SpriteFont _fontSprite;

    private Player _player;

    private PhysicsObject _testMonkey;

    // private MeshObject _skybox;
    private PhysicsObject _plane;

    private Skybox _skybox;

    private Vector2 _reticlePosition;
    private Texture2D _reticle;

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

    public T CreateNewGameObject<T>(Vector3? objectPosition = null, Quaternion? objectRotation = null,
        Vector3? objectScale = null) where T : GameObject
    {
        var createdGameObject = Activator.CreateInstance<T>();
        _gameObjects.Add(createdGameObject);
        if (createdGameObject is MeshObject meshObject)
        {
            _meshObjects.Add(meshObject);
        }

        createdGameObject.Initialize(objectPosition, objectRotation, objectScale);
        return createdGameObject;
    }

    public void RemoveGameObject(GameObject gameObjectToRemove)
    {
        if (!_gameObjects.Contains(gameObjectToRemove))
        {
            //todo: we are trying to remove a game object that is not registered in the game
            return;
        }

        if (gameObjectToRemove is MeshObject meshObject)
        {
            _meshObjects.Remove(meshObject);
        }

        _gameObjects.Remove(gameObjectToRemove);
        gameObjectToRemove.Dispose();
        //todo: should I clean something else
    }

    protected override void Initialize()
    {
        GraphicsDeviceManager.IsFullScreen = false;
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();

        GamePhysics.Initialize();

        DebugUtils.Initialize(GraphicsDevice);

        _meshObjects = new List<MeshObject>();
        _gameObjects = new List<GameObject>();

        _skybox = new Skybox("Textures/Skybox/SkyRed", Content);

        _plane = CreateNewGameObject<PhysicsObject>(Vector3.Zero,
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

        _player = CreateNewGameObject<Player>(new Vector3(0f, 20f, 0f), Quaternion.Identity);
        _player.RigidBody.Mass = 100f;

        _reticle = Content.Load<Texture2D>("Textures/UI/reticle");
        _reticlePosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);

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

        GamePhysics.UpdatePhysics(gameTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _skybox.Draw();

        foreach (var meshObject in _meshObjects)
        {
            meshObject.Draw(GraphicsDevice, gameTime);
        }

        DebugUtils.Draw(GraphicsDevice, gameTime);

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

        base.Draw(gameTime);
    }
}