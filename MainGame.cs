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
using Velentr.Font;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/c-sharp-tutorials
public class MainGame : Microsoft.Xna.Framework.Game
{
    public const int MaxRenderDistance = 20;

    public static Texture2D SinglePixelTexture;
    public static Texture2D SquareOutlineTexture;

    public static MainGame GameInstance;

    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    public static JsonSerializerSettings JsonSerializerSettings { get; private set; }
    public static JsonSerializer JsonSerializer { get; private set; }

    public static SpriteFont DefaultFont => GameInstance._defaultSpriteFont;

    public static GamePhysics Physics { private set; get; }

    public SceneManager SceneManager;

    private FrameCounter _frameCounter = new();

    private float _deltaTime;

    private SceneData _testSceneData;

    private SpriteFont _defaultSpriteFont;

    private Texture2D _cursorTexture;

    private UIGraphics _testGraphics;

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
                new ColorConverter(),
                new PointConverter()
            }
        };
        JsonSerializer = JsonSerializer.CreateDefault(JsonSerializerSettings);

        DebugUtils.Initialize(GraphicsDevice);
        
        SinglePixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        SinglePixelTexture.SetData([Color.White]);

        // initialize the physics engine for the game
        Physics = new GamePhysics();

        // initialize the scene manager for the game
        SceneManager = new SceneManager(Content);
        
        _defaultSpriteFont = Content.Load<SpriteFont>("Fonts/myFont");

        var mainMenu = SceneManager.Load("MainMenu");

        var canvas = mainMenu.GetGameObjectOfType<Canvas>();

        _testGraphics = canvas.GetGraphicByName<UIImage>("SpinningBoy");
        
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
        DebugUtils.PrintMessage("Game Initialized...");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        SquareOutlineTexture = Content.Load<Texture2D>("Textures/Primitives/square-outline");
        _cursorTexture = Content.Load<Texture2D>("Textures/UI/cursor");
        DebugUtils.PrintMessage("Content loaded...");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _testGraphics.Rotation += 2f * _deltaTime;

        SceneManager.UpdateScenes(_deltaTime);

        Physics.UpdatePhysics(_deltaTime);

        DebugUtils.Update(_deltaTime);

        base.Update(gameTime);
    }

    private SpriteBatch _testSpriteBatch;

    protected override void Draw(GameTime gameTime)
    {
        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _frameCounter.Update(_deltaTime);

        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        SceneManager.DrawScenes(gameTime);


        // draws some test lines for the Ui and a test cursor
        _testSpriteBatch ??= new SpriteBatch(GraphicsDevice);
        _testSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);

        _testSpriteBatch.Draw(SinglePixelTexture,
            new Rectangle(GraphicsDevice.Viewport.Width / 2, 0, 1, GraphicsDevice.Viewport.Height),
            Color.Gray);
        _testSpriteBatch.Draw(SinglePixelTexture,
            new Rectangle(0, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width, 1),
            Color.Gray);
        var mouseState = Mouse.GetState();
        _testSpriteBatch.Draw(_cursorTexture, new Rectangle(mouseState.X, mouseState.Y, 32, 32), Color.White);
        var cursorPositionString = $"X:{mouseState.X} Y:{mouseState.Y}";
        var cursorStringPosition = new Vector2(mouseState.X + 32, mouseState.Y);
        var measuredString = _defaultSpriteFont.MeasureString(cursorPositionString);
        _testSpriteBatch.Draw(SinglePixelTexture,
            new Rectangle((int)cursorStringPosition.X, (int)cursorStringPosition.Y, (int)measuredString.X + 2, (int)measuredString.Y),
            new Color(255, 255, 255, 0));
        _testSpriteBatch.DrawString(_defaultSpriteFont, cursorPositionString, cursorStringPosition, Color.Green);

        _testSpriteBatch.End();

        DebugUtils.DrawDebugAxis(Vector3.Zero);
        DebugUtils.Draw(GraphicsDevice, _deltaTime);

        base.Draw(gameTime);
    }
}