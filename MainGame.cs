using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TestMonoGame.Data;
using TestMonoGame.Debug;
using TestMonoGame.Game;
using TestMonoGame.Game.SceneManagement;
using TestMonoGame.Physics;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/c-sharp-tutorials
public class MainGame : Microsoft.Xna.Framework.Game
{
    public const int MaxRenderDistance = 20;

    public static MainGame GameInstance;
    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }
    
    public static JsonSerializerSettings JsonSerializerSettings { get; private set; }

    public static GamePhysics Physics { private set; get; }

    public SceneManager SceneManager;

    private FrameCounter _frameCounter = new();

    private float _deltaTime;

    private SceneData _testSceneData;

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

    protected override void Initialize()
    {
        GraphicsDeviceManager.IsFullScreen = false;
        GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        GraphicsDeviceManager.PreferredBackBufferHeight = 720;
        GraphicsDeviceManager.ApplyChanges();

        JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new FieldContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new Vector3Converter(),
                new Vector2Converter(),
                new Vector3NullConverter(),
                new Vector2NullConverter()
            }
        };

        DebugUtils.Initialize(GraphicsDevice);

        // initialize the physics engine for the game
        Physics = new GamePhysics();

        // initialize the scene manager for the game
        SceneManager = new SceneManager(Content);

        var loadedScene = SceneManager.Load("MainScene", Physics);

        var cubeModel = Content.Load<Model>("Models/cube");
        
        for (var x = 0; x < 100; x++)
        {
            for (var z = 0; z < 100; z++)
            {
                var cube = new PhysicsObject($"cube{x}/{z}");
                cube.Transform.Position = new Vector3(x + 0.5f, 0.5f, z + 0.5f);
                cube.IsStatic = true;
                cube.IsAffectedByGravity = false;
                cube.Model = cubeModel;
                loadedScene.AddNewGameObject(cube);
            }
        }

        // _reticle = Content.Load<Texture2D>("Textures/UI/reticle");
        // _reticlePosition = new Vector2(GraphicsDevice.Viewport.Width / 2f - _reticle.Width / 2f,
        //     GraphicsDevice.Viewport.Height / 2f - _reticle.Height / 2f);

        // _mainWorld = new World(123456789, 20, 20);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        // _spriteBatch = new SpriteBatch(GraphicsDevice);
        //
        // FallSfx = Content.Load<SoundEffect>("Audio/fall-hurt");
        // WalkSfx = Content.Load<SoundEffect>("Audio/footstep");
        // PlaceBlockSfx = Content.Load<SoundEffect>("Audio/place-block");
        // RemoveBlockSfx = Content.Load<SoundEffect>("Audio/remove-block");
        //
        //
        // _fontSprite = Content.Load<SpriteFont>("Fonts/myFont");
        // Debug.WriteLine(_fontSprite.Characters.Count);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        SceneManager.UpdateScenes(_deltaTime);

        Physics.UpdatePhysics(_deltaTime);

        DebugUtils.Update(_deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(_deltaTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        SceneManager.DrawScenes(gameTime);

        DebugUtils.DrawDebugAxis(Vector3.Zero);
        DebugUtils.Draw(GraphicsDevice, _deltaTime);

        base.Draw(gameTime);
    }
}