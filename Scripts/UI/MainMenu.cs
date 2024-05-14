using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Game.UI;

namespace BotanicaGame.Scripts.UI;

public class MainMenu : IExternalScript
{
    private Scene _mainMenuScene;
    private Scene _playScene;

    private UIGraphic _spinningCube;

    private bool _isInitialized;

    public bool IsInitialized() => _isInitialized;
    
    public void Start(GameObject gameObjectContext)
    {
        // retrieve the scene from the object that this script was attached to
        _mainMenuScene = gameObjectContext.SceneContext;

        // get the spinning cube object
        _spinningCube = _mainMenuScene.GetGameObjectByName<UIImage>("SpinningBoy");

        // quit the game when we click on the exit button
        _mainMenuScene.GetGameObjectByName<UIButton>("Play Button").OnButtonClicked += PlayGame;
        _mainMenuScene.GetGameObjectByName<UIButton>("Exit Button").OnButtonClicked += MainGame.GameInstance.Exit;
        _mainMenuScene.GetGameObjectByName<UIButton>("Settings Button").OnButtonClicked += MainGame.GameInstance.ScreenController.ToggleBorderless;

        _isInitialized = true;
    }

    public void Update(float deltaTime)
    {
        _spinningCube.Rotation += 2f * deltaTime;
    }

    public void PlayGame()
    {
        // load the main scene
        MainGame.SceneManager.Load("MainScene", MainGame.Physics);
        MainGame.SceneManager.Unload(_mainMenuScene);
    }
}