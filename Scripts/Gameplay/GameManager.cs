using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;

namespace BotanicaGame.Scripts.Gameplay;

public class GameManager : IExternalScript
{
    public bool IsInitialized() => true;
    
    private Scene _gameHudScene;

    public void Start(GameObject gameObjectContext)
    {
        // load the gamehud scene
        _gameHudScene = MainGame.SceneManager.Load("GameHud");
        
        MainGame.HideCursor = true;
    }

    public void Update(float deltaTime)
    {
        
    }
}