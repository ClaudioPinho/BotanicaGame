using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Game.UI;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Scripts.UI;

public class PauseMenu : IExternalScript
{
    private KeyboardState _previousKeyboardState;

    private UIGraphic _pauseMenu;

    private Scene _scene;
    
    public bool IsInitialized() => true;
    
    public void Start(GameObject gameObjectContext)
    {
        _scene = gameObjectContext.SceneContext;
        _pauseMenu = _scene.GetGameObjectByName<UIGraphic>("PauseMenu");
        _scene.GetGameObjectByName<UIButton>("Home Button").OnButtonClicked += ReturnHome;
        MainGame.LockCursor = true;
    }

    public void Update(float deltaTime)
    {
        if (_previousKeyboardState.IsKeyUp(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _pauseMenu.IsActive = !_pauseMenu.IsActive;
            MainGame.HideCursor = MainGame.LockCursor = !_pauseMenu.IsActive;
        }
        
        _previousKeyboardState = Keyboard.GetState();
    }
    
    private void ReturnHome()
    {
        // todo: temporary
        MainGame.SceneManager.Unload(MainGame.SceneManager.GetScene("MainScene"));
        MainGame.SceneManager.Unload(_scene);
        MainGame.SceneManager.Load("MainMenu");
    }

}