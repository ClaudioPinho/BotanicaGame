using BotanicaGame.Game;
using BotanicaGame.Scripts.UI;

namespace BotanicaGame.Scripts;

public class Main : IExternalScript
{
    private bool _initialized;

    public bool IsInitialized() => _initialized;

    public void Start(object context)
    {
        // add the main menu script at the start of the game
        MainGame.GameInstance.AddExternalScript(new MainMenu());
        
        _initialized = true;
    }

    public void Update(float deltaTime)
    {
        
    }
}