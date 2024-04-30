using System.Collections.Generic;
using BotanicaGame.Data;
using BotanicaGame.Debug;
using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Physics;
using BotanicaGame.Scripts;
using BotanicaGame.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace BotanicaGame;

// http://rbwhitaker.wikidot.com/c-sharp-tutorials
public class MainGame : Microsoft.Xna.Framework.Game
{
    public const int MaxRenderDistance = 20;

    public static Texture2D SinglePixelTexture;
    public static Texture2D SquareOutlineTexture;

    public static MainGame GameInstance;

    public ScreenController ScreenController { get; private set; }
    
    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    public static JsonSerializerSettings JsonSerializerSettings { get; private set; }
    public static JsonSerializer JsonSerializer { get; private set; }

    public static SpriteFont DefaultFont => GameInstance._defaultSpriteFont;

    public static GamePhysics Physics { private set; get; }

    public SceneManager SceneManager;

    private FrameCounter _frameCounter = new();

    private float _deltaTime;

    private SpriteFont _defaultSpriteFont;

    private Texture2D _cursorTexture;

    private List<IExternalScript> _externalScripts = [];

    private GameAudio _gameAudio;
    
    private MouseState _mouseState;
    private KeyboardState _keyboardState;
    private KeyboardState _previousKeyboardState;
    
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

    public void AddExternalScript(IExternalScript externalScript)
    {
        if (_externalScripts.Contains(externalScript)) return;
        _externalScripts.Add(externalScript);
        externalScript.Start(this);
    }

    public void RemoveExternalScript(IExternalScript externalScript)
    {
        if (!_externalScripts.Contains(externalScript)) return;
        _externalScripts.Remove(externalScript);
    }

    protected override void Initialize()
    {
        ScreenController = new ScreenController(GraphicsDeviceManager, Window);
        
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
        SinglePixelTexture.SetData([new Color(255, 255, 255, 255)]);

        // initialize the game audio with the group volumes
        _gameAudio = new GameAudio(new Dictionary<GameAudio.EAudioGroup, float>
        {
            { GameAudio.EAudioGroup.None, 1f },
            { GameAudio.EAudioGroup.UI, 0.5f },
            { GameAudio.EAudioGroup.Background, 1f },
            { GameAudio.EAudioGroup.Enemy, 1f },
            { GameAudio.EAudioGroup.Friendly, 1f },
        }, 1f);

        // initialize the physics engine for the game
        Physics = new GamePhysics();

        // initialize the scene manager for the game
        SceneManager = new SceneManager(Content);
        
        _defaultSpriteFont = Content.Load<SpriteFont>("Fonts/myFont");

        // the main script will be the entry point of the game, so it should be the first script to be loaded
        AddExternalScript(new Main());

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
        _keyboardState = Keyboard.GetState();
        
        // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        //     Keyboard.GetState().IsKeyDown(Keys.Escape))
        //     Exit();
        
        if (_keyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
        {
            ScreenController.ToggleBorderless();
        }

        _deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        SceneManager.UpdateScenes(_deltaTime);

        Physics.UpdatePhysics(_deltaTime);
        
        foreach (var externalScript in _externalScripts)
        {
            externalScript.Update(_deltaTime);
        }
        
        _gameAudio.PerformAudioCleanup();
        
        DebugUtils.Update(_deltaTime);

        _previousKeyboardState = _keyboardState;

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

        // _testSpriteBatch.Draw(SinglePixelTexture,
        //     new Rectangle(GraphicsDevice.Viewport.Width / 2, 0, 1, GraphicsDevice.Viewport.Height),
        //     Color.Gray);
        // _testSpriteBatch.Draw(SinglePixelTexture,
        //     new Rectangle(0, GraphicsDevice.Viewport.Height / 2, GraphicsDevice.Viewport.Width, 1),
        //     Color.Gray);
        _mouseState = Mouse.GetState();
        _testSpriteBatch.Draw(_cursorTexture, new Rectangle(_mouseState.X, _mouseState.Y, 32, 32), Color.White);
        var cursorPositionString = $"X:{_mouseState.X} Y:{_mouseState.Y}";
        var cursorStringPosition = new Vector2(_mouseState.X + 32, _mouseState.Y);
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