using System;
using BotanicaGame.Game;
using BotanicaGame.Game.UI;
using BotanicaGame.Scripts.Gameplay;
using BotanicaGame.Utils;
using Microsoft.Xna.Framework;

namespace BotanicaGame.Scripts.UI;

public class GameHud : IExternalScript
{
    private UIImage _oxygenFillBar;
    private UIText _oxygenValueTag;

    private UIImage _healthFillBar;
    private UIText _healthValueTag;

    public bool IsInitialized() => true;

    public void Start(GameObject gameObjectContext)
    {
        _oxygenFillBar = gameObjectContext.SceneContext.GetGameObjectByName<UIImage>("O2bar-fill");
        _oxygenValueTag = gameObjectContext.SceneContext.GetGameObjectByName<UIText>("O2bar-value");

        _healthFillBar = gameObjectContext.SceneContext.GetGameObjectByName<UIImage>("Healthbar-fill");
        _healthValueTag = gameObjectContext.SceneContext.GetGameObjectByName<UIText>("Healthbar-value");
    }

    public void Update(float deltaTime)
    {
        // todo: eventually the hud updates should not be processed every frame but based on events
        if (Player.LocalPlayer == null) return;

        _oxygenValueTag.Text = Player.LocalPlayer.OxygenLevel.ToString("00");
        _oxygenFillBar.Scale = new Vector2(1,
            MathExtensions.Lerp(0, 1, MathExtensions.InverseLerp(0, Player.LocalPlayer.MaxOxygenLevel, Player.LocalPlayer.OxygenLevel)));

        _healthValueTag.Text = Player.LocalPlayer.Entity.Health.ToString("00");
        _healthFillBar.Scale = new Vector2(1,
            MathExtensions.Lerp(0, 1, MathExtensions.InverseLerp(0, Player.LocalPlayer.Entity.MaxHealth, Player.LocalPlayer.Entity.Health)));
    }

}