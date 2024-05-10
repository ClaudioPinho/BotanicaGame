using BotanicaGame.Debug;
using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Game.UI;

namespace BotanicaGame.Scripts.UI;

public class MainMenu : IExternalScript
{
    private Scene _mainMenuScene;
    private Scene _playScene;

    private Canvas _mainCanvas;

    private UIGraphic _spinningCube;

    private bool _isInitialized;

    public bool IsInitialized() => _isInitialized;

    public void Start(object context)
    {
        // load the scene
        _mainMenuScene = MainGame.GameInstance.SceneManager.Load("MainMenu");
        
        // get the main canvas from the scene
        _mainCanvas = _mainMenuScene.GetGameObjectOfType<Canvas>();
        
        // get the spinning cube from the canvas
        _spinningCube = _mainCanvas.GetGraphicByName<UIImage>("SpinningBoy");
        
        // quit the game when we click on the exit button
        _mainCanvas.GetGraphicByName<UIButton>("Exit Button").OnButtonClicked += MainGame.GameInstance.Exit;
        // _mainCanvas.GetGraphicByName<UIButton>("Settings Button").OnButtonClicked += MainGame.GameInstance.ScreenController.ToggleBorderless;

        _isInitialized = true;
    }

    public void Update(float deltaTime)
    {
        _spinningCube.Rotation += 2f * deltaTime;
    }
}