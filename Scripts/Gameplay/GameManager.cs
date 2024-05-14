using BotanicaGame.Game;

namespace BotanicaGame.Scripts.Gameplay;

public class GameManager : IExternalScript
{
    public bool IsInitialized() => true;

    public void Start(GameObject gameObjectContext)
    {
        MainGame.HideCursor = true;
    }

    public void Update(float deltaTime)
    {
        
    }
}