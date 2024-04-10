using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    public const int MaxRenderDistance = 20;

    public static MainGame GameInstance;
    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    public static GamePhysics Physics { private set; get; }

    // public Texture2D GrassTexture;
    public Model CubeModel;

    public SoundEffect FallSfx;
    public SoundEffect WalkSfx;
    public SoundEffect PlaceBlockSfx;
    public SoundEffect RemoveBlockSfx;

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
    private Queue<GameObject> _gameObjectsToAdd;
    private Queue<GameObject> _gameObjectsToRemove;
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

    public void AddNewGameObject(GameObject gameObject)
    {
        _gameObjectsToAdd.Enqueue(gameObject);
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        _gameObjectsToRemove.Enqueue(gameObject);
    }

    public void Play3DAudio(AudioEmitter emitter, SoundEffect audio, float maxDistance, float volume)
    {
        // todo: currently only the player is able to listen for the audio but I might need to change this in the future
        var audioListeners = _gameObjects.Where(x => x is Player)
            .Select(x => ((Player)x).AudioListener)
            .ToArray();

        var soundInstance = audio.CreateInstance();
        soundInstance.Apply3D(audioListeners, emitter);
        soundInstance.Volume = volume;
        soundInstance.Play();
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
        _gameObjectsToAdd = new Queue<GameObject>();
        _gameObjectsToRemove = new Queue<GameObject>();

        _skybox = new Skybox("Textures/Skybox/SkyRed", Content);

        CubeModel = Content.Load<Model>("Models/cube");
        // GrassTexture = Content.Load<Texture2D>("Textures/Blocks/grass");

        // _plane = new PhysicsObject("WorldPlane", true, false, new Vector3(200, 1f, 200f), new Vector3(0, 0, 0),
        //     Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0), new Vector3(100f, 100f, 1f));
        // _plane.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        // _plane.Model = Content.Load<Model>("Models/Primitives/plane");
        // _plane.Texture = sandTexture;
        // _plane.TextureTiling = Vector2.One * 20f;
        // _plane.CollisionOffset = new Vector3(0, -.5f, 0);

        // _testMonkey = new PhysicsObject("The monkey", false, false, null, new Vector3(5f, 5f, 0f),
        //     Quaternion.CreateFromYawPitchRoll(0f, MathF.PI / 4, 0f));
        // _testMonkey.MeshEffect = new GenericEffectAdapter(Content.Load<Effect>("Effects/TilingEffect"));
        // _testMonkey.Model = Content.Load<Model>("Models/monkey");
        // _testMonkey.Texture = Content.Load<Texture2D>("Textures/wooden-box");
        // _testMonkey.TextureTiling = Vector2.One * 2f;
        // _testMonkey.DiffuseColor = Color.Green;


        _player = new Player("Main player", new Vector3(0f, 4f, 0f), Quaternion.Identity);
        _player.DebugDrawCollision = true;

        var dummyEntity = new Entity("Dummy", true, new Vector3(1, 2, 1),
            new Vector3(5, 20, 5));
        dummyEntity.Model = Content.Load<Model>("Models/Player/player");
        dummyEntity.CollisionOffset = new Vector3(0, dummyEntity.CollisionSize.Y / 2, 0f);


        // AddGameObject(_plane);
        // AddGameObject(_testMonkey);
        // AddGameObject(cubeTest);
        // AddGameObject(cubeTest2);
        // AddGameObject(cubeTest3);
        AddGameObject(_player);
        // AddGameObject(dummyEntity);

        for (var x = 0; x < 1; x++)
        {
            for (var z = 0; z < 1; z++)
            {
                var cube = new PhysicsObject($"cube{x}/{z}", true, false, position: new Vector3(x, 0, z));
                cube.Model = CubeModel;
                // cube.Texture = GrassTexture;
                AddGameObject(cube);
            }
        }

        _reticle = Content.Load<Texture2D>("Textures/UI/reticle");
        _reticlePosition = new Vector2(GraphicsDevice.Viewport.Width / 2f - _reticle.Width / 2f,
            GraphicsDevice.Viewport.Height / 2f - _reticle.Height / 2f);

        _mainWorld = new World(123456789, 20, 20);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        FallSfx = Content.Load<SoundEffect>("Audio/fall-hurt");
        WalkSfx = Content.Load<SoundEffect>("Audio/footstep");
        PlaceBlockSfx = Content.Load<SoundEffect>("Audio/place-block");
        RemoveBlockSfx = Content.Load<SoundEffect>("Audio/remove-block");

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

        // update the physics simulation
        Physics.UpdatePhysics(deltaTime);
        // Physics.UpdatePhysics(1f/100f);

        _mainWorld.Update(gameTime);

        // add any object that we marked for being added
        while (_gameObjectsToAdd.Count != 0)
        {
            AddGameObject(_gameObjectsToAdd.Dequeue());
        }

        foreach (var gameObject in _gameObjects)
        {
            // todo: does it make sense to stop processing entities outside player view?
            if (Vector3.Distance(gameObject.Transform.Position, _player.Transform.Position) >
                MaxRenderDistance) continue;
            gameObject.Update(deltaTime);
        }

        // remove the objects marked for clearing
        while (_gameObjectsToRemove.Count != 0)
        {
            RemoveGameObject(_gameObjectsToRemove.Dequeue());
        }

        DebugUtils.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(deltaTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        // // Create a new SamplerState object
        // SamplerState samplerState = new SamplerState();
        //
        // // Set the filter mode to point sampling
        // samplerState.Filter = TextureFilter.Point;
        //
        // // Set the address mode to clamp
        // samplerState.AddressU = TextureAddressMode.Clamp;
        // samplerState.AddressV = TextureAddressMode.Clamp;
        //
        // // Set the GraphicsDevice's SamplerState
        // GraphicsDevice.SamplerStates[0] = samplerState;

        _skybox.Draw();

        // _mainWorld.Draw(GraphicsDevice, gameTime);

        foreach (var meshObject in _meshObjects)
        {
            if (Vector3.Distance(meshObject.Transform.Position, _player.Transform.Position) >
                MaxRenderDistance) continue;
            meshObject.Draw(GraphicsDevice, gameTime);
        }

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_fontSprite, $"FPS: {_frameCounter.CurrentFramesPerSecond}", Vector2.Zero,
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite, $"Player position: {_player.Transform.Position}", new Vector2(0f, 20f),
            Color.Yellow);
        _spriteBatch.DrawString(_fontSprite, $"Camera position: {_player.Camera.Transform.Position}",
            new Vector2(0f, 40f), Color.Yellow);
        _spriteBatch.DrawString(_fontSprite,
            $"Player velocity: {_player.Velocity}", new Vector2(0f, 60f), Color.Yellow);
        _spriteBatch.DrawString(_fontSprite, $"Player health: {_player.Health}", new Vector2(0f, 80f), Color.Yellow);
        _spriteBatch.Draw(_reticle, _reticlePosition, Color.White);

        _spriteBatch.End();

        DebugUtils.DrawDebugAxis(Vector3.Zero);

        DebugUtils.Draw(GraphicsDevice, gameTime);

        base.Draw(gameTime);
    }

    private void AddGameObject(GameObject gameObject)
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

    private void RemoveGameObject(GameObject gameObject)
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
}