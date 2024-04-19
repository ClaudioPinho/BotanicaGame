using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TestMonoGame.Data;
using TestMonoGame.Debug;
using TestMonoGame.Game.SceneManagement;
using TestMonoGame.Game.UI;
using TestMonoGame.Physics;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/c-sharp-tutorials
public class MainGame : Microsoft.Xna.Framework.Game
{
    public const int MaxRenderDistance = 20;

    public static Texture2D SinglePixelTexture;

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
                new Vector2NullConverter(),
                new ColorConverter()
            }
        };

        DebugUtils.Initialize(GraphicsDevice);

        SinglePixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        SinglePixelTexture.SetData([Color.White]);

        // initialize the physics engine for the game
        Physics = new GamePhysics();

        // initialize the scene manager for the game
        SceneManager = new SceneManager(Content);

        var mainMenu = SceneManager.Load("MainMenu");

        var canvas = mainMenu.GetGameObjectOfType<Canvas>();
        
        // var loadedScene = SceneManager.Load("MainScene", Physics);
        //
        // var canvas = new Canvas("Player Canvas");
        //
        // var image = new UIImage(Content.Load<Texture2D>("Textures/UI/reticle"));
        // image.Destination.Location = new Point(100, 100);
        //
        // canvas.AddUIGraphic(image);
        //
        // loadedScene.AddNewGameObject(canvas);
        
        // var cubeModel = Content.Load<Model>("Models/cube");
        // for (var x = 0; x < 100; x++)
        // {
        //     for (var z = 0; z < 100; z++)
        //     {
        //         var cube = new PhysicsObject($"cube{x}/{z}");
        //         cube.Transform.Position = new Vector3(x + 0.5f, 0.5f, z + 0.5f);
        //         cube.IsStatic = true;
        //         cube.IsAffectedByGravity = false;
        //         cube.Model = cubeModel;
        //         loadedScene.AddNewGameObject(cube);
        //     }
        // }

        // _mainWorld = new World(123456789, 20, 20);

        base.Initialize();
    }

    protected override void LoadContent()
    {
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