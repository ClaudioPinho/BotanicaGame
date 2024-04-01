using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TestMonoGame.Extensions;
using TestMonoGame.Game;

namespace TestMonoGame;

// http://rbwhitaker.wikidot.com/monogame-collision-detection

public class MainGame : Microsoft.Xna.Framework.Game
{
    public static MainGame GameInstance;

    public static GraphicsDeviceManager GraphicsDeviceManager { private set; get; }

    private SpriteBatch _spriteBatch;
    private SpriteFont _fontSprite;

    private Player _player;
    private MeshObject _testCube;
    // private MeshObject _skybox;
    private MeshObject _plane;

    private Skybox _skybox;
    
    private Vector2 _reticlePosition;
    private Texture2D _reticle;

    private List<GameObject> _gameObjects;
    private List<MeshObject> _meshObjects;

    public MainGame()
    {
        GameInstance = this;
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    public T CreateNewGameObject<T>() where T : GameObject
    {
        var createdGameObject = Activator.CreateInstance<T>();
        _gameObjects.Add(createdGameObject);
        if (createdGameObject is MeshObject meshObject)
        {
            _meshObjects.Add(meshObject);
        }

        createdGameObject.Initialize();
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

        _meshObjects = new List<MeshObject>();
        _gameObjects = new List<GameObject>();

        _skybox = new Skybox("Textures/Skybox/SkyRed", Content);

        _plane = CreateNewGameObject<MeshObject>();
        _plane.Model = Content.Load<Model>("Models/Primitives/plane");
        _plane.Texture = Content.Load<Texture2D>("Textures/Ground/Grass_02");
        _plane.Transform.Scale = new Vector3(100f, 1f, 100f);
        _plane.Transform.Rotation.SetEulerAngles(0f, -90f, 0f);

        _testCube = CreateNewGameObject<PhysicsObject>();
        _testCube.Transform.Position.X = 5f;
        _testCube.Transform.Position.Y = 1f;
        _testCube.Model = Content.Load<Model>("Models/cube");
        _testCube.Texture = Content.Load<Texture2D>("Textures/wooden-box");
        _testCube.DiffuseColor = Color.Red;

        _player = CreateNewGameObject<Player>();
        
        _reticle = Content.Load<Texture2D>("Textures/UI/reticle");
        _reticlePosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);

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
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        _skybox.Draw();
        
        foreach (var meshObject in _meshObjects)
        {
            meshObject.Draw(gameTime);
        }

        _spriteBatch.Begin();
        
        _spriteBatch.DrawString(_fontSprite, $"Player position| {_player.Transform.Position.ToString()}", Vector2.Zero,
            Color.Red);
        _spriteBatch.DrawString(_fontSprite,
            $"Player rotation| {_player.Transform.Rotation.ToEuler().ToString()}", new Vector2(0f, 20f),
            Color.Red);
        _spriteBatch.Draw(_reticle, _reticlePosition, Color.White);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}