namespace BotanicaGame.Game;

public interface IExternalScript
{
    /// <summary>
    /// This should return the initialization state of the external script in order to prevent
    /// other objects from initializing it again and again
    /// </summary>
    /// <returns></returns>
    public bool IsInitialized();
    
    /// <summary>
    /// This is called when the object this script is attached to is trying to initialize this script
    /// </summary>
    /// <param name="context"></param>
    public void Start(object context);
    
    /// <summary>
    /// This is called for every object that has this script attached
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime);
}