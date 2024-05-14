using BotanicaGame.Game;
using BotanicaGame.Game.SceneManagement;
using BotanicaGame.Game.UI;
using Microsoft.Xna.Framework.Input;

namespace BotanicaGame.Scripts.UI;

public class GameHud : IExternalScript
{
    private KeyboardState _previousKeyboardState;

    private UIGraphic _hud;

    private Scene _scene;
    
    public bool IsInitialized() => true;
    
    public void Start(GameObject gameObjectContext)
    {
        _scene = gameObjectContext.SceneContext;
        _hud = _scene.GetGameObjectByName<UIGraphic>("GameHUD");
        _scene.GetGameObjectByName<UIButton>("Home Button").OnButtonClicked += ReturnHome;
        MainGame.LockCursor = true;
    }

    public void Update(float deltaTime)
    {
        if (_previousKeyboardState.IsKeyUp(Keys.Escape) && Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            _hud.IsActive = !_hud.IsActive;
            MainGame.HideCursor = MainGame.LockCursor = !_hud.IsActive;
        }
        
        _previousKeyboardState = Keyboard.GetState();
    }
    
    private void ReturnHome()
    {
        MainGame.SceneManager.Load("MainMenu");
        MainGame.SceneManager.Unload(_scene);
    }

}