using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Scripts.UI;

namespace BotanicaGame.Scripts;

public class Main : IExternalScript
{
    private bool _initialized;

    public bool IsInitialized() => _initialized;

    private Scene _mainMenuScene;

    public void Start(GameObject gameObjectContext)
    {
        // load the main menu scene when the game starts
        _mainMenuScene = MainGame.SceneManager.Load("MainMenu");
        _initialized = true;
    }

    public void Update(float deltaTime)
    {
        
    }
}